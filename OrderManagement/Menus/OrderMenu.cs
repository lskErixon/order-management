using OrderManagement.Helpers;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagement.Menus;

public class OrderMenu
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;

    public OrderMenu(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("ORDERS");
            Console.WriteLine("1. List all orders");
            Console.WriteLine("2. Show order details");
            Console.WriteLine("3. Create new order");
            Console.WriteLine("4. Update order status");
            Console.WriteLine("5. Delete order");
            Console.WriteLine("0. Back");
            Console.Write("\nSelect option: ");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1": await ListAllAsync(); break;
                case "2": await ShowDetailsAsync(); break;
                case "3": await CreateAsync(); break;
                case "4": await UpdateStatusAsync(); break;
                case "5": await DeleteAsync(); break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option"); break;
            }
        }
    }

    private async Task ListAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        Console.WriteLine($"\n{"ID",-5} {"Customer",-20} {"Total",-12} {"Status",-12} {"Date",-20}");
        ConsoleHelper.PrintLine(75);
        foreach (var o in orders)
        {
            Console.WriteLine($"{o.Id,-5} {o.CustomerName,-20} {o.TotalAmount,-12:F2} {o.Status,-12} {o.CreatedAt,-20:g}");
        }
    }

    private async Task ShowDetailsAsync()
    {
        var id = ConsoleHelper.ReadInt("Enter order ID: ");
        if (id == null) return;

        var order = await _orderRepository.GetByIdAsync(id.Value);
        if (order == null)
        {
            ConsoleHelper.PrintError("Order not found");
            return;
        }

        Console.WriteLine($"\n=== ORDER #{order.Id} ===");
        Console.WriteLine($"Customer: {order.CustomerName}");
        Console.WriteLine($"Status: {order.Status}");
        Console.WriteLine($"Note: {order.Note ?? "-"}");
        Console.WriteLine($"Date: {order.CreatedAt}");
        Console.WriteLine($"\nItems:");
        Console.WriteLine($"{"Product",-25} {"Qty",-5} {"Price",-10} {"Total",-10}");
        ConsoleHelper.PrintLine(55);
        foreach (var item in order.Items)
        {
            Console.WriteLine($"{item.ProductName,-25} {item.Quantity,-5} {item.UnitPrice,-10:F2} {item.TotalPrice,-10:F2}");
        }
        ConsoleHelper.PrintLine(55);
        Console.WriteLine($"{"TOTAL:",-42} {order.TotalAmount:F2}");
    }

    private async Task CreateAsync()
    {
        Console.WriteLine("\n=== CREATE NEW ORDER ===");

        // Выбор клиента
        var users = await _userRepository.GetAllAsync();
        Console.WriteLine("\nAvailable customers:");
        foreach (var u in users)
        {
            Console.WriteLine($"  {u.Id} - {u.Name} ({u.Email})");
        }

        var userId = ConsoleHelper.ReadInt("Select customer ID: ");
        if (userId == null)
        {
            ConsoleHelper.PrintError("Invalid customer ID");
            return;
        }

        // Выбор товаров
        var products = await _productRepository.GetAllAsync();
        Console.WriteLine("\nAvailable products:");
        foreach (var p in products.Where(p => p.IsAvailable && p.StockQuantity > 0))
        {
            Console.WriteLine($"  {p.Id} - {p.Name} (${p.Price:F2}, Stock: {p.StockQuantity})");
        }

        var order = new Order
        {
            UserId = userId.Value,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>()
        };

        while (true)
        {
            var productId = ConsoleHelper.ReadInt("\nAdd product ID (or 0 to finish): ");
            if (productId == null || productId == 0) break;

            var product = await _productRepository.GetByIdAsync(productId.Value);
            if (product == null)
            {
                ConsoleHelper.PrintError("Product not found");
                continue;
            }

            var qty = ConsoleHelper.ReadInt($"Quantity (max {product.StockQuantity}): ");
            if (qty == null || qty <= 0 || qty > product.StockQuantity)
            {
                ConsoleHelper.PrintError("Invalid quantity");
                continue;
            }

            order.Items.Add(new OrderItem
            {
                ProductId = productId.Value,
                Quantity = qty.Value,
                UnitPrice = product.Price
            });

            ConsoleHelper.PrintSuccess($"Added: {product.Name} x {qty}");
        }

        if (order.Items.Count == 0)
        {
            ConsoleHelper.PrintError("Order must have at least one item");
            return;
        }

        order.Note = ConsoleHelper.ReadString("Note (optional): ");
        order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

        Console.WriteLine($"\nTotal: ${order.TotalAmount:F2}");

        if (!ConsoleHelper.Confirm("Confirm order?")) return;

        var newOrderId = await _orderRepository.CreateAsync(order);
        ConsoleHelper.PrintSuccess($"Order created with ID: {newOrderId}");
    }

    private async Task UpdateStatusAsync()
    {
        var id = ConsoleHelper.ReadInt("Enter order ID: ");
        if (id == null) return;

        Console.WriteLine("Status options: Pending, Processing, Shipped, Delivered, Cancelled");
        Console.Write("New status: ");

        if (Enum.TryParse(Console.ReadLine(), true, out OrderStatus status))
        {
            if (await _orderRepository.UpdateStatusAsync(id.Value, status))
                ConsoleHelper.PrintSuccess("Status updated");
            else
                ConsoleHelper.PrintError("Update failed");
        }
        else
        {
            ConsoleHelper.PrintError("Invalid status");
        }
    }

    private async Task DeleteAsync()
    {
        var id = ConsoleHelper.ReadInt("Enter order ID to delete: ");
        if (id == null) return;

        if (!ConsoleHelper.Confirm("Are you sure?")) return;

        if (await _orderRepository.DeleteAsync(id.Value))
            ConsoleHelper.PrintSuccess("Order deleted");
        else
            ConsoleHelper.PrintError("Delete failed");
    }
}