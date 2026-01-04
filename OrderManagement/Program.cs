using OrderManagement.Data;
using OrderManagement.Menus;
using OrderManagement.Repositories;
using OrderManagement.Services;

namespace OrderManagement;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("===========================================");
        Console.WriteLine("    ORDER MANAGEMENT SYSTEM");
        Console.WriteLine("    Repository Pattern (D1)");
        Console.WriteLine("===========================================\n");

        // Inicializace
        DbConnectionFactory connectionFactory;
        try
        {
            Console.WriteLine("Loading configuration...");
            connectionFactory = new DbConnectionFactory();

            Console.WriteLine("Testing database connection...");
            if (!connectionFactory.TestConnection(out string error))
            {
                Console.WriteLine($"[ERROR] Database connection failed: {error}");
                Console.WriteLine("\nCheck your appsettings.json configuration.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("[OK] Database connected successfully!\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Initialization failed: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        // Inicializace repozitářů
        var userRepository = new UserRepository(connectionFactory);
        var categoryRepository = new CategoryRepository(connectionFactory);
        var productRepository = new ProductRepository(connectionFactory);
        var orderRepository = new OrderRepository(connectionFactory);
        var reportRepository = new ReportRepository(connectionFactory);
        var importService = new ImportService(userRepository, productRepository, categoryRepository);

        // Inicializace menu
        var userMenu = new UserMenu(userRepository);
        var categoryMenu = new CategoryMenu(categoryRepository);
        var productMenu = new ProductMenu(productRepository, categoryRepository);
        var orderMenu = new OrderMenu(orderRepository, userRepository, productRepository);
        var reportMenu = new ReportMenu(reportRepository);
        var importMenu = new ImportMenu(importService);

        // Hlavní smyčka
        while (true)
        {
            Console.WriteLine("\n============ MAIN MENU ============");
            Console.WriteLine("1. Users");
            Console.WriteLine("2. Categories");
            Console.WriteLine("3. Products");
            Console.WriteLine("4. Orders");
            Console.WriteLine("5. Reports");
            Console.WriteLine("6. Import Data (CSV)");
            Console.WriteLine("0. Exit");
            Console.Write("\nSelect option: ");

            var input = Console.ReadLine();

            try
            {
                switch (input)
                {
                    case "1": await userMenu.ShowAsync(); break;
                    case "2": await categoryMenu.ShowAsync(); break;
                    case "3": await productMenu.ShowAsync(); break;
                    case "4": await orderMenu.ShowAsync(); break;
                    case "5": await reportMenu.ShowAsync(); break;
                    case "6": await importMenu.ShowAsync(); break;
                    case "0":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("[ERROR] Invalid option");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }
    }
}