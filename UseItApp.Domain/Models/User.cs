namespace UseItApp.Domain.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public List<Item> OwnedItems { get; set; } = [];
    public List<Loan> BorrowedItems { get; set; } = [];
    
    public bool IsBlocked { get; set; } = false;
    public string? BlockReason { get; set; }
    public DateTime? BlockedUntil { get; set; }
}