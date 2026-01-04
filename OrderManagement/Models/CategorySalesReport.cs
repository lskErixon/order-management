namespace OrderManagement.Models;

public class CategorySalesReport
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}