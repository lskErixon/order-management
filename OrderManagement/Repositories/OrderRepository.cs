using MySqlConnector;
using OrderManagement.Data;
using OrderManagement.Models;

namespace OrderManagement.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public OrderRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        var orders = new List<Order>();

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"SELECT o.Id, o.UserId, o.TotalAmount, o.Status, o.Note, o.CreatedAt, u.Name AS CustomerName
              FROM Orders o
              JOIN Users u ON o.UserId = u.Id
              ORDER BY o.CreatedAt DESC",
            connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orders.Add(MapReaderToOrder(reader));
        }

        return orders;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        // Získat objednávku
        using var orderCommand = new MySqlCommand(
            @"SELECT o.Id, o.UserId, o.TotalAmount, o.Status, o.Note, o.CreatedAt, u.Name AS CustomerName
              FROM Orders o
              JOIN Users u ON o.UserId = u.Id
              WHERE o.Id = @Id",
            connection);
        orderCommand.Parameters.AddWithValue("@Id", id);

        Order? order = null;

        using (var reader = await orderCommand.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                order = MapReaderToOrder(reader);
            }
        }

        if (order == null) return null;

        // Získat položky objednávky
        using var itemsCommand = new MySqlCommand(
            @"SELECT oi.Id, oi.OrderId, oi.ProductId, oi.Quantity, oi.UnitPrice, p.Name AS ProductName
              FROM OrderItems oi
              JOIN Products p ON oi.ProductId = p.Id
              WHERE oi.OrderId = @OrderId",
            connection);
        itemsCommand.Parameters.AddWithValue("@OrderId", id);

        using (var reader = await itemsCommand.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                order.Items.Add(MapReaderToOrderItem(reader));
            }
        }

        return order;
    }

    public async Task<List<Order>> GetByUserIdAsync(int userId)
    {
        var orders = new List<Order>();

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"SELECT o.Id, o.UserId, o.TotalAmount, o.Status, o.Note, o.CreatedAt, u.Name AS CustomerName
              FROM Orders o
              JOIN Users u ON o.UserId = u.Id
              WHERE o.UserId = @UserId
              ORDER BY o.CreatedAt DESC",
            connection);
        command.Parameters.AddWithValue("@UserId", userId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orders.Add(MapReaderToOrder(reader));
        }

        return orders;
    }

    // ========== CREATE (ТРАНЗАКЦИЯ - вставка в несколько таблиц) ==========
    public async Task<int> CreateAsync(Order order)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // 1. Вставляем заказ
            using var orderCommand = new MySqlCommand(
                @"INSERT INTO Orders (UserId, TotalAmount, Status, Note, CreatedAt) 
                  VALUES (@UserId, @TotalAmount, @Status, @Note, @CreatedAt);
                  SELECT LAST_INSERT_ID();",
                connection, transaction);

            orderCommand.Parameters.AddWithValue("@UserId", order.UserId);
            orderCommand.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
            orderCommand.Parameters.AddWithValue("@Status", order.Status.ToString());
            orderCommand.Parameters.AddWithValue("@Note", order.Note ?? (object)DBNull.Value);
            orderCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            var orderId = Convert.ToInt32(await orderCommand.ExecuteScalarAsync());

            // 2. Вставляем позиции заказа
            foreach (var item in order.Items)
            {
                using var itemCommand = new MySqlCommand(
                    @"INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice) 
                      VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice)",
                    connection, transaction);

                itemCommand.Parameters.AddWithValue("@OrderId", orderId);
                itemCommand.Parameters.AddWithValue("@ProductId", item.ProductId);
                itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                itemCommand.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);

                await itemCommand.ExecuteNonQueryAsync();

                // 3. Уменьшаем количество на складе
                using var stockCommand = new MySqlCommand(
                    "UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE Id = @ProductId",
                    connection, transaction);

                stockCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                stockCommand.Parameters.AddWithValue("@ProductId", item.ProductId);

                await stockCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, OrderStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            "UPDATE Orders SET Status = @Status WHERE Id = @Id",
            connection);

        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Status", status.ToString());

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        // OrderItems удалятся автоматически (ON DELETE CASCADE)
        using var command = new MySqlCommand(
            "DELETE FROM Orders WHERE Id = @Id",
            connection);
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private Order MapReaderToOrder(MySqlDataReader reader)
    {
        return new Order
        {
            Id = reader.GetInt32("Id"),
            UserId = reader.GetInt32("UserId"),
            TotalAmount = reader.GetDecimal("TotalAmount"),
            Status = Enum.Parse<OrderStatus>(reader.GetString("Status")),
            Note = reader.IsDBNull(reader.GetOrdinal("Note"))
                ? null
                : reader.GetString("Note"),
            CreatedAt = reader.GetDateTime("CreatedAt"),
            CustomerName = reader.GetString("CustomerName")
        };
    }

    private OrderItem MapReaderToOrderItem(MySqlDataReader reader)
    {
        return new OrderItem
        {
            Id = reader.GetInt32("Id"),
            OrderId = reader.GetInt32("OrderId"),
            ProductId = reader.GetInt32("ProductId"),
            Quantity = reader.GetInt32("Quantity"),
            UnitPrice = reader.GetDecimal("UnitPrice"),
            ProductName = reader.GetString("ProductName")
        };
    }
}