namespace OrderManagement.Models;

public class SalesReport
{
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProductsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime ReportDate { get; set; }
}