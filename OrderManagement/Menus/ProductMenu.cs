using OrderManagement.Helpers;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagement.Menus;

public class ProductMenu
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductMenu(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("PRODUCTS");
            Console.WriteLine("1. List all products");
            Console.WriteLine("2. Show product details");
            Console.WriteLine("3. Create product");
            Console.WriteLine("4. Update product");
            Console.WriteLine("5. Delete product");
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
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option"); break;
            }
        }
    }

    private async Task ListAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        Console.WriteLine($"\n{"ID",-5} {"Name",-25} {"Category",-15} {"Price",-10} {"Stock",-7} {"Available",-9}");
        ConsoleHelper.PrintLine(75);
        foreach (var p in products)
        {
            Console.WriteLine($"{p.Id,-5} {p.Name,-25} {p.CategoryName,-15} {p.Price,-10:F2} {p.StockQuantity,-7} {p.IsAvailable,-9}");
        }
    }

    private async Task ShowDetailsAsync()
    {
        var id = ConsoleHelper.ReadInt("Enter product ID: ");
        if (id == null) return;

        var product = await _productRepository.GetByIdAsync(id.Value);
        if (product == null)
        {
            ConsoleHelper.PrintError("Product not found");
            return;
        }

        Console.WriteLine($"\nID: {product.Id}");
        Console.WriteLine($"Name: {product.Name}");
        Console.WriteLine($"Category: {product.CategoryName}");
        Console.WriteLine($"Description: {product.Description}");
        Console.WriteLine($"Price: {product.Price:F2}");
        Console.WriteLine($"Stock: {product.StockQuantity}");
        Console.WriteLine($"Available: {product.IsAvailable}");
    }

    private async Task CreateAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        Console.WriteLine("\nAvailable categories:");
        foreach (var c in categories)
        {
            Console.WriteLine($"  {c.Id} - {c.Name}");
        }

        var product = new Product
        {
            Name = ConsoleHelper.ReadString("Name: "),
            CategoryId = ConsoleHelper.ReadInt("Category ID: ") ?? 0,
            Description = ConsoleHelper.ReadString("Description: "),
            Price = ConsoleHelper.ReadDecimal("Price: ") ?? 0,
            StockQuantity = ConsoleHelper.ReadInt("Stock Quantity: ") ?? 0,
            IsAvailable = true
        };

        var id = await _productRepository.CreateAsync(product);
        ConsoleHelper.PrintSuccess($"Product created with ID: {id}");
    }

    private async Task UpdateAsync()
    {
        var id = ConsoleHelper.ReadInt("Enter product ID to update: ");
        if (id == null) return;

        var product = await _productRepository.GetByIdAsync(id.Value);
        if (product == null)
        {
            ConsoleHelper.PrintError("Product not found");
            return;
        }

        Console.Write($"Name [{product.Name}]: ");
        var name = Console.ReadLine();
        if (!string.IsNullOrEmpty(name)) product.Name = name;

        Console.Write($"Price [{product.Price}]: ");
        var priceStr = Console.ReadLine();
        if (decimal.TryParse(priceStr, out decimal price)) product.Price = price;

        Console.Write($"Stock [{product.StockQuantity}]: ");
        var stockStr = Console.ReadLine();
        if (int.TryParse(stockStr, out int stock)) product.StockQuantity = stock;

        Console.Write($"Available [{product.IsAvailable}] (true/false): ");
        var availStr = Console.ReadLine();
        if (bool.TryParse(availStr, out bool avail)) product.IsAvailable = avail;

        if (await _productRepository.UpdateAsync(product))
            ConsoleHelper.PrintSuccess("Product updated");
        else
            ConsoleHelper.PrintError("Update failed");
    }

    private async Task DeleteAsync()
    {
        var id = ConsoleHelper.ReadInt("Enter product ID to delete: ");
        if (id == null) return;

        if (!ConsoleHelper.Confirm("Are you sure?")) return;

        if (await _productRepository.DeleteAsync(id.Value))
            ConsoleHelper.PrintSuccess("Product deleted");
        else
            ConsoleHelper.PrintError("Delete failed (product may be in orders)");
    }
}
