namespace OrderManagement.Services;

public class ImportResult
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public interface IImportService
{
    Task<ImportResult> ImportUsersFromCsvAsync(string filePath);
    Task<ImportResult> ImportProductsFromCsvAsync(string filePath);
}