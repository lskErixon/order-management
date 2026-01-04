using MySqlConnector;
using OrderManagement.Data;
using OrderManagement.Models;

namespace OrderManagement.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public ProductRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        var products = new List<Product>();
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"SELECT p.Id, p.CategoryId, p.Name, p.Description, p.Price, 
                     p.StockQuantity, p.IsAvailable, p.CreatedAt, c.Name AS CategoryName
              FROM Products p
              JOIN Categories c ON p.CategoryId = c.Id",
            connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(MapReaderToProduct(reader));
        }

        return products;
    }

    public async Task<List<Product>> GetByCategoryIdAsync(int categoryId)
    {
        var products = new List<Product>();

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"SELECT p.Id, p.CategoryId, p.Name, p.Description, p.Price, 
                     p.StockQuantity, p.IsAvailable, p.CreatedAt, c.Name AS CategoryName
              FROM Products p
              JOIN Categories c ON p.CategoryId = c.Id
              WHERE p.CategoryId = @CategoryId",
            connection);
        command.Parameters.AddWithValue("@CategoryId", categoryId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(MapReaderToProduct(reader));
        }

        return products;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"SELECT p.Id, p.CategoryId, p.Name, p.Description, p.Price, 
                     p.StockQuantity, p.IsAvailable, p.CreatedAt, c.Name AS CategoryName
              FROM Products p
              JOIN Categories c ON p.CategoryId = c.Id
              WHERE p.Id = @Id",
            connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapReaderToProduct(reader);
        }

        return null;
    }

    public async Task<int> CreateAsync(Product product)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"INSERT INTO Products (CategoryId, Name, Description, Price, StockQuantity, IsAvailable, CreatedAt) 
              VALUES (@CategoryId, @Name, @Description, @Price, @StockQuantity, @IsAvailable, @CreatedAt);
              SELECT LAST_INSERT_ID();",
            connection);

        command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
        command.Parameters.AddWithValue("@IsAvailable", product.IsAvailable);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"UPDATE Products 
              SET CategoryId = @CategoryId, Name = @Name, Description = @Description,
                  Price = @Price, StockQuantity = @StockQuantity, IsAvailable = @IsAvailable 
              WHERE Id = @Id",
            connection);

        command.Parameters.AddWithValue("@Id", product.Id);
        command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
        command.Parameters.AddWithValue("@IsAvailable", product.IsAvailable);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            "DELETE FROM Products WHERE Id = @Id",
            connection);
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateStockAsync(int id, int quantity)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            "UPDATE Products SET StockQuantity = StockQuantity + @Quantity WHERE Id = @Id",
            connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Quantity", quantity);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private Product MapReaderToProduct(MySqlDataReader reader)
    {
        return new Product
        {
            Id = reader.GetInt32("Id"),
            CategoryId = reader.GetInt32("CategoryId"),
            Name = reader.GetString("Name"),
            Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                ? null
                : reader.GetString("Description"),
            Price = reader.GetDecimal("Price"),
            StockQuantity = reader.GetInt32("StockQuantity"),
            IsAvailable = reader.GetBoolean("IsAvailable"),
            CreatedAt = reader.GetDateTime("CreatedAt"),
            CategoryName = reader.GetString("CategoryName")
        };
    }
}