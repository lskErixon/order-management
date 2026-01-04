using OrderManagement.Models;

namespace OrderManagement.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<int> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<bool> TransferBonusPointsAsync(int fromUserId, int toUserId, decimal amount);
}