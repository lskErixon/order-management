using OrderManagement.Models;

namespace OrderManagement.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> GetByCategoryIdAsync(int categoryId);
    Task<Product?> GetByIdAsync(int id);
    Task<int> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateStockAsync(int id, int quantity);
}