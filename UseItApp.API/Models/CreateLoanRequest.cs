namespace UseItApp.API.Models;

public class CreateLoanRequest
{
    public int ItemId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Notes { get; set; }
}