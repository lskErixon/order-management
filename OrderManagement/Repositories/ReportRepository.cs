using MySqlConnector;
using OrderManagement.Data;
using OrderManagement.Models;

namespace OrderManagement.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public ReportRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // ========== HLAVNÍ REPORT (3+ tabulky: Orders, OrderItems, Users, Products) ==========
    public async Task<SalesReport> GetSalesReportAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"SELECT 
                (SELECT COUNT(*) FROM Orders) AS TotalOrders,
                (SELECT COUNT(DISTINCT UserId) FROM Orders) AS TotalCustomers,
                (SELECT COALESCE(SUM(Quantity), 0) FROM OrderItems) AS TotalProductsSold,
                (SELECT COALESCE(SUM(TotalAmount), 0) FROM Orders) AS TotalRevenue,
                (SELECT COALESCE(AVG(TotalAmount), 0) FROM Orders) AS AverageOrderValue",
            connection);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new SalesReport
            {
                TotalOrders = reader.GetInt32("TotalOrders"),
                TotalCustomers = reader.GetInt32("TotalCustomers"),
                TotalProductsSold = Convert.ToInt32(reader.GetDecimal("TotalProductsSold")),
                TotalRevenue = reader.GetDecimal("TotalRevenue"),
                AverageOrderValue = reader.GetDecimal("AverageOrderValue"),
                ReportDate = DateTime.Now
            };
        }

        return new SalesReport { ReportDate = DateTime.Now };
    }

    // ========== TOP PRODUKTY (Products, OrderItems, Categories) ==========
    public async Task<List<TopProductReport>> GetTopProductsAsync(int top = 5)
    {
        var products = new List<TopProductReport>();

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"SELECT 
                p.Id AS ProductId,
                p.Name AS ProductName,
                c.Name AS CategoryName,
                COALESCE(SUM(oi.Quantity), 0) AS QuantitySold,
                COALESCE(SUM(oi.Quantity * oi.UnitPrice), 0) AS TotalRevenue
              FROM Products p
              JOIN Categories c ON p.CategoryId = c.Id
              LEFT JOIN OrderItems oi ON p.Id = oi.ProductId
              GROUP BY p.Id, p.Name, c.Name
              ORDER BY QuantitySold DESC
              LIMIT @Top",
            connection);
        command.Parameters.AddWithValue("@Top", top);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(new TopProductReport
            {
                ProductId = reader.GetInt32("ProductId"),
                ProductName = reader.GetString("ProductName"),
                CategoryName = reader.GetString("CategoryName"),
                QuantitySold = Convert.ToInt32(reader.GetDecimal("QuantitySold")),
                TotalRevenue = reader.GetDecimal("TotalRevenue")
            });
        }

        return products;
    }

    // ========== PRODEJE PODLE KATEGORIÍ (VIEW) ==========
    public async Task<List<CategorySalesReport>> GetCategorySalesAsync()
    {
        var categories = new List<CategorySalesReport>();

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        // Používáme VIEW vw_CategorySales
        using var command = new MySqlCommand(
            "SELECT CategoryId, CategoryName, ProductCount, TotalSold, TotalRevenue FROM vw_CategorySales",
            connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            categories.Add(new CategorySalesReport
            {
                CategoryId = reader.GetInt32("CategoryId"),
                CategoryName = reader.GetString("CategoryName"),
                ProductCount = reader.GetInt32("ProductCount"),
                TotalSold = Convert.ToInt32(reader.GetDecimal("TotalSold")),
                TotalRevenue = reader.GetDecimal("TotalRevenue")
            });
        }

        return categories;
    }

    // ========== TOP ZÁKAZNÍCI (Users, Orders) ==========
    public async Task<List<CustomerReport>> GetTopCustomersAsync(int top = 5)
    {
        var customers = new List<CustomerReport>();

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"SELECT 
                u.Id AS UserId,
                u.Name AS CustomerName,
                u.Email,
                u.BonusPoints,
                COUNT(o.Id) AS OrderCount,
                COALESCE(SUM(o.TotalAmount), 0) AS TotalSpent
              FROM Users u
              LEFT JOIN Orders o ON u.Id = o.UserId
              GROUP BY u.Id, u.Name, u.Email, u.BonusPoints
              ORDER BY TotalSpent DESC
              LIMIT @Top",
            connection);
        command.Parameters.AddWithValue("@Top", top);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            customers.Add(new CustomerReport
            {
                UserId = reader.GetInt32("UserId"),
                CustomerName = reader.GetString("CustomerName"),
                Email = reader.GetString("Email"),
                BonusPoints = reader.GetDecimal("BonusPoints"),
                OrderCount = reader.GetInt32("OrderCount"),
                TotalSpent = reader.GetDecimal("TotalSpent")
            });
        }

        return customers;
    }
}