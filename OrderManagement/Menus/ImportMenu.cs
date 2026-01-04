using OrderManagement.Helpers;
using OrderManagement.Services;

namespace OrderManagement.Menus;

public class ImportMenu
{
    private readonly IImportService _importService;

    public ImportMenu(IImportService importService)
    {
        _importService = importService;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("IMPORT DATA");
            Console.WriteLine("1. Import Users from CSV");
            Console.WriteLine("2. Import Products from CSV");
            Console.WriteLine("0. Back");
            Console.Write("\nSelect option: ");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1": await ImportUsersAsync(); break;
                case "2": await ImportProductsAsync(); break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option"); break;
            }
        }
    }

    private async Task ImportUsersAsync()
    {
        var path = ConsoleHelper.ReadString("Enter CSV file path: ");
        var result = await _importService.ImportUsersFromCsvAsync(path);
        PrintResult(result);
    }

    private async Task ImportProductsAsync()
    {
        var path = ConsoleHelper.ReadString("Enter CSV file path: ");
        var result = await _importService.ImportProductsFromCsvAsync(path);
        PrintResult(result);
    }

    private void PrintResult(ImportResult result)
    {
        Console.WriteLine($"\n[RESULT] Success: {result.SuccessCount}, Failed: {result.FailedCount}");
        if (result.Errors.Any())
        {
            Console.WriteLine("Errors:");
            foreach (var err in result.Errors)
            {
                Console.WriteLine($"  - {err}");
            }
        }
    }
}