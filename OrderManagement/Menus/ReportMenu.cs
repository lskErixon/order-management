using OrderManagement.Helpers;
using OrderManagement.Repositories;

namespace OrderManagement.Menus;

public class ReportMenu
{
    private readonly IReportRepository _reportRepository;

    public ReportMenu(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("REPORTS");
            Console.WriteLine("1. Sales Summary Report");
            Console.WriteLine("2. Top Products");
            Console.WriteLine("3. Category Sales");
            Console.WriteLine("4. Top Customers");
            Console.WriteLine("0. Back");
            Console.Write("\nSelect option: ");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1": await SalesSummaryAsync(); break;
                case "2": await TopProductsAsync(); break;
                case "3": await CategorySalesAsync(); break;
                case "4": await TopCustomersAsync(); break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option"); break;
            }
        }
    }

    private async Task SalesSummaryAsync()
    {
        var sales = await _reportRepository.GetSalesReportAsync();
        Console.WriteLine("\n=== SALES SUMMARY REPORT ===");
        Console.WriteLine($"Report Date: {sales.ReportDate:g}");
        Console.WriteLine($"Total Orders: {sales.TotalOrders}");
        Console.WriteLine($"Total Customers: {sales.TotalCustomers}");
        Console.WriteLine($"Total Products Sold: {sales.TotalProductsSold}");
        Console.WriteLine($"Total Revenue: ${sales.TotalRevenue:F2}");
        Console.WriteLine($"Average Order Value: ${sales.AverageOrderValue:F2}");
    }

    private async Task TopProductsAsync()
    {
        var products = await _reportRepository.GetTopProductsAsync(5);
        Console.WriteLine("\n=== TOP 5 PRODUCTS ===");
        Console.WriteLine($"{"#",-3} {"Product",-25} {"Category",-15} {"Sold",-8} {"Revenue",-12}");
        ConsoleHelper.PrintLine();
        int rank = 1;
        foreach (var p in products)
        {
            Console.WriteLine($"{rank++,-3} {p.ProductName,-25} {p.CategoryName,-15} {p.QuantitySold,-8} ${p.TotalRevenue,-12:F2}");
        }
    }

    private async Task CategorySalesAsync()
    {
        var categories = await _reportRepository.GetCategorySalesAsync();
        Console.WriteLine("\n=== CATEGORY SALES ===");
        Console.WriteLine($"{"Category",-20} {"Products",-10} {"Sold",-10} {"Revenue",-12}");
        ConsoleHelper.PrintLine(55);
        foreach (var c in categories)
        {
            Console.WriteLine($"{c.CategoryName,-20} {c.ProductCount,-10} {c.TotalSold,-10} ${c.TotalRevenue,-12:F2}");
        }
    }

    private async Task TopCustomersAsync()
    {
        var customers = await _reportRepository.GetTopCustomersAsync(5);
        Console.WriteLine("\n=== TOP 5 CUSTOMERS ===");
        Console.WriteLine($"{"#",-3} {"Name",-20} {"Orders",-8} {"Total Spent",-12} {"Bonus",-10}");
        ConsoleHelper.PrintLine(55);
        int rank = 1;
        foreach (var c in customers)
        {
            Console.WriteLine($"{rank++,-3} {c.CustomerName,-20} {c.OrderCount,-8} ${c.TotalSpent,-12:F2} {c.BonusPoints,-10:F2}");
        }
    }
}