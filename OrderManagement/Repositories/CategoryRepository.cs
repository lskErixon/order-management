using MySqlConnector;
using OrderManagement.Data;
using OrderManagement.Models;

namespace OrderManagement.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public CategoryRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        var categories = new List<Category>();
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            "SELECT Id, Name, Description, IsActive, CreatedAt FROM Categories",
            connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            categories.Add(MapReaderToCategory(reader));
        }

        return categories;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            "SELECT Id, Name, Description, IsActive, CreatedAt FROM Categories WHERE Id = @Id",
            connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapReaderToCategory(reader);
        }

        return null;
    }

    public async Task<int> CreateAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"INSERT INTO Categories (Name, Description, IsActive, CreatedAt) 
              VALUES (@Name, @Description, @IsActive, @CreatedAt);
              SELECT LAST_INSERT_ID();",
            connection);

        command.Parameters.AddWithValue("@Name", category.Name);
        command.Parameters.AddWithValue("@Description", category.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", category.IsActive);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"UPDATE Categories 
              SET Name = @Name, Description = @Description, IsActive = @IsActive 
              WHERE Id = @Id",
            connection);

        command.Parameters.AddWithValue("@Id", category.Id);
        command.Parameters.AddWithValue("@Name", category.Name);
        command.Parameters.AddWithValue("@Description", category.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", category.IsActive);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            "DELETE FROM Categories WHERE Id = @Id",
            connection);
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private Category MapReaderToCategory(MySqlDataReader reader)
    {
        return new Category
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                ? null 
                : reader.GetString("Description"),
            IsActive = reader.GetBoolean("IsActive"),
            CreatedAt = reader.GetDateTime("CreatedAt")
        };
    }
}