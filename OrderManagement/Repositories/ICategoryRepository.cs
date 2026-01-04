using OrderManagement.Models;

namespace OrderManagement.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<int> CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(int id);
}