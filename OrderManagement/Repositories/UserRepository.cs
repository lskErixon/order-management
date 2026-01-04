using MySqlConnector;
using OrderManagement.Data;
using OrderManagement.Models;

namespace OrderManagement.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public UserRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // ========== GET ALL ==========
    public async Task<List<User>> GetAllAsync()
    {
        var users = new List<User>();

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            "SELECT Id, Name, Email, BonusPoints, IsActive, Role, CreatedAt FROM Users",
            connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(MapReaderToUser(reader));
        }

        return users;
    }

    // ========== GET BY ID ==========
    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            "SELECT Id, Name, Email, BonusPoints, IsActive, Role, CreatedAt FROM Users WHERE Id = @Id",
            connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapReaderToUser(reader);
        }

        return null;
    }

    // ========== CREATE ==========
    public async Task<int> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"INSERT INTO Users (Name, Email, BonusPoints, IsActive, Role, CreatedAt) 
              VALUES (@Name, @Email, @BonusPoints, @IsActive, @Role, @CreatedAt);
              SELECT LAST_INSERT_ID();",
            connection);

        command.Parameters.AddWithValue("@Name", user.Name);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@BonusPoints", user.BonusPoints);
        command.Parameters.AddWithValue("@IsActive", user.IsActive);
        command.Parameters.AddWithValue("@Role", user.Role.ToString());
        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    // ========== UPDATE ==========
    public async Task<bool> UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            @"UPDATE Users 
              SET Name = @Name, Email = @Email, BonusPoints = @BonusPoints, 
                  IsActive = @IsActive, Role = @Role 
              WHERE Id = @Id",
            connection);

        command.Parameters.AddWithValue("@Id", user.Id);
        command.Parameters.AddWithValue("@Name", user.Name);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@BonusPoints", user.BonusPoints);
        command.Parameters.AddWithValue("@IsActive", user.IsActive);
        command.Parameters.AddWithValue("@Role", user.Role.ToString());

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    // ========== DELETE ==========
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new MySqlCommand(
            "DELETE FROM Users WHERE Id = @Id",
            connection);
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    // ========== TRANSFER BONUS POINTS ==========
    public async Task<bool> TransferBonusPointsAsync(int fromUserId, int toUserId, decimal amount)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            using var checkCommand = new MySqlCommand(
                "SELECT BonusPoints FROM Users WHERE Id = @Id",
                connection, transaction);
            checkCommand.Parameters.AddWithValue("@Id", fromUserId);

            var balance = (decimal?)await checkCommand.ExecuteScalarAsync();
            if (balance == null || balance < amount)
            {
                await transaction.RollbackAsync();
                return false;
            }
            
            using var deductCommand = new MySqlCommand(
                "UPDATE Users SET BonusPoints = BonusPoints - @Amount WHERE Id = @Id",
                connection, transaction);
            deductCommand.Parameters.AddWithValue("@Amount", amount);
            deductCommand.Parameters.AddWithValue("@Id", fromUserId);
            await deductCommand.ExecuteNonQueryAsync();
            
            using var addCommand = new MySqlCommand(
                "UPDATE Users SET BonusPoints = BonusPoints + @Amount WHERE Id = @Id",
                connection, transaction);
            addCommand.Parameters.AddWithValue("@Amount", amount);
            addCommand.Parameters.AddWithValue("@Id", toUserId);
            await addCommand.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ========== HELPER: ==========
    private User MapReaderToUser(MySqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email"),
            BonusPoints = reader.GetDecimal("BonusPoints"),
            IsActive = reader.GetBoolean("IsActive"),
            Role = Enum.Parse<UserRole>(reader.GetString("Role")),
            CreatedAt = reader.GetDateTime("CreatedAt")
        };
    }
}
