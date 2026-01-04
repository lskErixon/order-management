using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagement.Services;

public class ImportService : IImportService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ImportService(
        IUserRepository userRepository,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _userRepository = userRepository;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    // ========== IMPORT USERS ==========
    public async Task<ImportResult> ImportUsersFromCsvAsync(string filePath)
    {
        var result = new ImportResult();

        if (!File.Exists(filePath))
        {
            result.Errors.Add($"File not found: {filePath}");
            return result;
        }

        var lines = await File.ReadAllLinesAsync(filePath);

        if (lines.Length < 2)
        {
            result.Errors.Add("CSV file is empty or has no data rows");
            return result;
        }
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            try
            {
                var columns = ParseCsvLine(line);

                if (columns.Length < 4)
                {
                    result.Errors.Add($"Line {i + 1}: Invalid format (expected 4 columns)");
                    result.FailedCount++;
                    continue;
                }

                var user = new User
                {
                    Name = columns[0].Trim(),
                    Email = columns[1].Trim(),
                    BonusPoints = decimal.Parse(columns[2].Trim()),
                    Role = Enum.Parse<UserRole>(columns[3].Trim(), ignoreCase: true),
                    IsActive = true
                };

                // Валидация
                if (string.IsNullOrEmpty(user.Name))
                {
                    result.Errors.Add($"Line {i + 1}: Name is required");
                    result.FailedCount++;
                    continue;
                }

                if (string.IsNullOrEmpty(user.Email) || !user.Email.Contains("@"))
                {
                    result.Errors.Add($"Line {i + 1}: Invalid email");
                    result.FailedCount++;
                    continue;
                }

                await _userRepository.CreateAsync(user);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Line {i + 1}: {ex.Message}");
                result.FailedCount++;
            }
        }

        return result;
    }

    // ========== IMPORT PRODUCTS ==========
    public async Task<ImportResult> ImportProductsFromCsvAsync(string filePath)
    {
        var result = new ImportResult();

        if (!File.Exists(filePath))
        {
            result.Errors.Add($"File not found: {filePath}");
            return result;
        }

        var lines = await File.ReadAllLinesAsync(filePath);

        if (lines.Length < 2)
        {
            result.Errors.Add("CSV file is empty or has no data rows");
            return result;
        }

        var categories = await _categoryRepository.GetAllAsync();
        var categoryDict = categories.ToDictionary(c => c.Name.ToLower(), c => c.Id);

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            try
            {
                var columns = ParseCsvLine(line);

                if (columns.Length < 5)
                {
                    result.Errors.Add($"Line {i + 1}: Invalid format (expected 5 columns)");
                    result.FailedCount++;
                    continue;
                }

                var categoryName = columns[1].Trim().ToLower();

                if (!categoryDict.ContainsKey(categoryName))
                {
                    result.Errors.Add($"Line {i + 1}: Category '{columns[1]}' not found");
                    result.FailedCount++;
                    continue;
                }

                var product = new Product
                {
                    Name = columns[0].Trim(),
                    CategoryId = categoryDict[categoryName],
                    Description = columns[2].Trim(),
                    Price = decimal.Parse(columns[3].Trim()),
                    StockQuantity = int.Parse(columns[4].Trim()),
                    IsAvailable = true
                };

                if (string.IsNullOrEmpty(product.Name))
                {
                    result.Errors.Add($"Line {i + 1}: Name is required");
                    result.FailedCount++;
                    continue;
                }

                if (product.Price <= 0)
                {
                    result.Errors.Add($"Line {i + 1}: Price must be greater than 0");
                    result.FailedCount++;
                    continue;
                }

                await _productRepository.CreateAsync(product);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Line {i + 1}: {ex.Message}");
                result.FailedCount++;
            }
        }

        return result;
    }

    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = "";
        var inQuotes = false;

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current);
        return result.ToArray();
    }
}