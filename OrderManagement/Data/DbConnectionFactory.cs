using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace OrderManagement.Data;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");
    }

    public MySqlConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }

    // Проверка подключения
    public bool TestConnection(out string errorMessage)
    {
        errorMessage = string.Empty;
        try
        {
            using var connection = CreateConnection();
            connection.Open();
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}