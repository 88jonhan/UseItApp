namespace UseItApp.Domain.Models;

public enum LoanStatus
{
    Requested,
    Approved,
    Rejected,
    Active,
    ReturnInitiated, 
    Returned,
    Overdue
}

public class Loan
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Requested;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int ItemId { get; set; }
    public int BorrowerId { get; set; }
    
    public Item Item { get; set; } = null!;
    public User Borrower { get; set; } = null!;
}