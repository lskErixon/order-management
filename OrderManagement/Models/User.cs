namespace OrderManagement.Models;

public enum UserRole
{
    Customer,
    Admin,
    Manager
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; } = String.Empty;
    public decimal BonusPoints { get; set; }
    public bool IsActive { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}