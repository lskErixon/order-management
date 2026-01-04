using OrderManagement.Helpers;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagement.Menus;

public class UserMenu
{
    private readonly IUserRepository _userRepository;

    public UserMenu(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("USERS");
            Console.WriteLine("1. List all users");
            Console.WriteLine("2. Show user details");
            Console.WriteLine("3. Create user");
            Console.WriteLine("4. Update user");
            Console.WriteLine("5. Delete user");
            Console.WriteLine("6. Transfer Bonus Points");
            Console.WriteLine("0. Back");
            Console.Write("\nSelect option: ");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1": await ListAllAsync(); break;
                case "2": await ShowDetailsAsync(); break;
                case "3": await CreateAsync(); break;
                case "4": await UpdateAsync(); break;
                case "5": await DeleteAsync(); break;
                case "6": await TransferBonusPointsAsync(); break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option"); break;
            }
        }
    }

    private async Task ListAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        Console.WriteLine($"\n{"ID",-5} {"Name",-20} {"Email",-30} {"Bonus",-10} {"Role",-10} {"Active",-6}");
        ConsoleHelper.PrintLine(85);
        foreach (var u in users)
        {
            Console.WriteLine($"{u.Id,-5} {u.Name,-20} {u.Email,-30} {u.BonusPoints,-10:F2} {u.Role,-10} {u.IsActive,-6}");
        }
    }

    private async Task ShowDetailsAsync()
    {
        var id = ConsoleHelper.ReadInt("Enter user ID: ");
        if (id == null) return;

        var user = await _userRepository.GetByIdAsync(id.Value);
        if (user == null)
        {
            ConsoleHelper.PrintError("User not found");
            return;
        }

        Console.WriteLine($"\nID: {user.Id}");
        Console.WriteLine($"Name: {user.Name}");
        Console.WriteLine($"Email: {user.Email}");
        Console.WriteLine($"Bonus Points: {user.BonusPoints:F2}");
        Console.WriteLine($"Role: {user.Role}");
        Console.WriteLine($"Active: {user.IsActive}");
        Console.WriteLine($"Created: {user.CreatedAt}");
    }

    private async Task CreateAsync()
    {
        var user = new User
        {
            Name = ConsoleHelper.ReadString("Name: "),
            Email = ConsoleHelper.ReadString("Email: "),
            BonusPoints = ConsoleHelper.ReadDecimal("Bonus Points: ") ?? 0,
            IsActive = true
        };

        Console.Write("Role (Customer/Admin/Manager): ");
        Enum.TryParse(Console.ReadLine(), true, out UserRole role);
        user.Role = role;

        var id = await _userRepository.CreateAsync(user);
        ConsoleHelper.PrintSuccess($"User created with ID: {id}");
    }

    private async Task UpdateAsync()
    {
        var id = ConsoleHelper.ReadInt("Enter user ID to update: ");
        if (id == null) return;

        var user = await _userRepository.GetByIdAsync(id.Value);
        if (user == null)
        {
            ConsoleHelper.PrintError("User not found");
            return;
        }

        Console.Write($"Name [{user.Name}]: ");
        var name = Console.ReadLine();
        if (!string.IsNullOrEmpty(name)) user.Name = name;

        Console.Write($"Email [{user.Email}]: ");
        var email = Console.ReadLine();
        if (!string.IsNullOrEmpty(email)) user.Email = email;

        Console.Write($"Bonus Points [{user.BonusPoints}]: ");
        var bonusStr = Console.ReadLine();
        if (decimal.TryParse(bonusStr, out decimal bonus)) user.BonusPoints = bonus;

        Console.Write($"Active [{user.IsActive}] (true/false): ");
        var activeStr = Console.ReadLine();
        if (bool.TryParse(activeStr, out bool isActive)) user.IsActive = isActive;

        if (await _userRepository.UpdateAsync(user))
            ConsoleHelper.PrintSuccess("User updated");
        else
            ConsoleHelper.PrintError("Update failed");
    }

    private async Task DeleteAsync()
    {
        var id = ConsoleHelper.ReadInt("Enter user ID to delete: ");
        if (id == null) return;

        if (!ConsoleHelper.Confirm("Are you sure?")) return;

        if (await _userRepository.DeleteAsync(id.Value))
            ConsoleHelper.PrintSuccess("User deleted");
        else
            ConsoleHelper.PrintError("Delete failed (user may have orders)");
    }

    private async Task TransferBonusPointsAsync()
    {
        Console.WriteLine("\n=== TRANSFER BONUS POINTS ===");

        var users = await _userRepository.GetAllAsync();
        Console.WriteLine("\nUsers:");
        foreach (var u in users)
        {
            Console.WriteLine($"  {u.Id} - {u.Name} (Bonus: {u.BonusPoints:F2})");
        }

        var fromId = ConsoleHelper.ReadInt("\nFrom user ID: ");
        var toId = ConsoleHelper.ReadInt("To user ID: ");
        var amount = ConsoleHelper.ReadDecimal("Amount: ");

        if (fromId == null || toId == null || amount == null || amount <= 0)
        {
            ConsoleHelper.PrintError("Invalid input");
            return;
        }

        if (!ConsoleHelper.Confirm($"Transfer {amount:F2} points from user {fromId} to user {toId}?"))
            return;

        if (await _userRepository.TransferBonusPointsAsync(fromId.Value, toId.Value, amount.Value))
            ConsoleHelper.PrintSuccess("Transfer completed successfully");
        else
            ConsoleHelper.PrintError("Transfer failed (insufficient balance or invalid users)");
    }
}