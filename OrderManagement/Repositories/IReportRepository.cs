using OrderManagement.Models;

namespace OrderManagement.Repositories;

public interface IReportRepository
{
    Task<SalesReport> GetSalesReportAsync();
    Task<List<TopProductReport>> GetTopProductsAsync(int top = 5);
    Task<List<CategorySalesReport>> GetCategorySalesAsync();
    Task<List<CustomerReport>> GetTopCustomersAsync(int top = 5);
}