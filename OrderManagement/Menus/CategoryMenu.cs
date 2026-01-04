using OrderManagement.Helpers;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagement.Menus;

public class CategoryMenu
{
    private readonly ICategoryRepository _categoryRepository;
    
    public CategoryMenu(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("CATEGORIES");
            Console.WriteLine("1. List all categories");
            Console.WriteLine("2. Create category");
            Console.WriteLine("3. Update category");
            Console.WriteLine("4. Delete category");
            Console.WriteLine("0. Back");
            Console.Write("\nSelect option: ");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1": await ListAllAsync(); break;
                case "2": await CreateAsync(); break;
                case "3": await UpdateAsync(); break;
                case "4": await DeleteAsync(); break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option"); break;
            }
        }
    }

    private async Task ListAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        Console.WriteLine($"\n{"ID",-5} {"Name",-20} {"Description",-30} {"Active",-6}");
        ConsoleHelper.PrintLine();
        foreach (var c in categories)
        {
            Console.WriteLine($"{c.Id,-5} {c.Name,-20} {c.Description ?? "",-30} {c.IsActive,-6}");
        }
    }

    private async Task CreateAsync()
    {
        var category = new Category
        {
            Name = ConsoleHelper.ReadString("Name: "),
            Description = ConsoleHelper.ReadString("Description: "),
            IsActive = true
        };

        var id = await _categoryRepository.CreateAsync(category);
        ConsoleHelper.PrintSuccess($"Category created with ID: {id}");
    }

    private async Task UpdateAsync()
    {
        var id = ConsoleHelper.ReadInt("Enter category ID to update: ");
        if (id == null) return;

        var category = await _categoryRepository.GetByIdAsync(id.Value);
        if (category == null)
        {
            ConsoleHelper.PrintError("Category not found");
            return;
        }

        Console.Write($"Name [{category.Name}]: ");
        var name = Console.ReadLine();
        if (!string.IsNullOrEmpty(name)) category.Name = name;

        Console.Write($"Description [{category.Description}]: ");
        var desc = Console.ReadLine();
        if (!string.IsNullOrEmpty(desc)) category.Description = desc;

        if (await _categoryRepository.UpdateAsync(category))
            ConsoleHelper.PrintSuccess("Category updated");
        else
            ConsoleHelper.PrintError("Update failed");
    }

    private async Task DeleteAsync()
    {
        var id = ConsoleHelper.ReadInt("Enter category ID to delete: ");
        if (id == null) return;

        if (!ConsoleHelper.Confirm("Are you sure?")) return;

        if (await _categoryRepository.DeleteAsync(id.Value))
            ConsoleHelper.PrintSuccess("Category deleted");
        else
            ConsoleHelper.PrintError("Delete failed (category may have products)");
    }
}