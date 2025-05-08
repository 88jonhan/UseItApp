namespace UseItApp.Domain.Models;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int OwnerId { get; set; }
    
    public User Owner { get; set; } = null!;
    public List<Loan> LoanHistory { get; set; } = new List<Loan>();
}