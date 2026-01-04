namespace OrderManagement.Models;

public class CustomerReport
{
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal BonusPoints { get; set; }
}