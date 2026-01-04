using OrderManagement.Models;

namespace OrderManagement.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<List<Order>> GetByUserIdAsync(int userId);
    Task<int> CreateAsync(Order order); // Вставка в Orders + OrderItems
    Task<bool> UpdateStatusAsync(int id, OrderStatus status);
    Task<bool> DeleteAsync(int id);
}