using Microsoft.EntityFrameworkCore;
using UseItApp.API.Interfaces;
using UseItApp.API.Models;
using UseItApp.Data;
using UseItApp.Domain.Enums;
using UseItApp.Domain.Models;

namespace UseItApp.API.Services;

public class LoanService(ApplicationDbContext dbContext) : ILoanService
{
    public async Task CheckOverdueLoans()
    {
        var now = DateTime.UtcNow;

        var overdueLoans = await dbContext.Loans
            .Include(l => l.Borrower)
            .Include(l => l.Item)
            .Where(l => (l.Status == LoanStatus.Active || l.Status == LoanStatus.ReturnInitiated)
                        && l.EndDate < now)
            .ToListAsync();

        foreach (var loan in overdueLoans)
        {
            loan.Status = LoanStatus.Overdue;

            loan.Borrower.IsBlocked = true;
            loan.Borrower.BlockReason = $"Förfallet lån av {loan.Item.Name}";
            loan.Borrower.BlockedUntil = now.AddDays(30);
        }

        await dbContext.SaveChangesAsync();
    }

    public bool IsStatusChangeAllowed(LoanStatus currentStatus, LoanStatus newStatus, bool isBorrower)
    {
        if (!isBorrower)
        {
            if (currentStatus == LoanStatus.Requested &&
                (newStatus == LoanStatus.Approved || newStatus == LoanStatus.Rejected))
                return true;

            if (currentStatus == LoanStatus.Approved && newStatus == LoanStatus.Active)
                return true;
        }
        else
        {
            if (currentStatus == LoanStatus.Active && newStatus == LoanStatus.Returned)
                return true;
        }

        return false;
    }

    public async Task<IEnumerable<Loan>> GetLoansForUserAsync(int userId)
    {
        return await dbContext.Loans
            .Include(l => l.Item)
            .Include(l => l.Borrower)
            .Where(l => l.BorrowerId == userId || l.Item.OwnerId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Loan>> GetUserLoansAsync(int userId)
    {
        return await dbContext.Loans
            .Include(l => l.Item)
            .ThenInclude(i => i.Owner)
            .Include(l => l.Borrower)
            .Where(l => l.BorrowerId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Loan>> GetOwnerLoansAsync(int userId)
    {
        return await dbContext.Loans
            .Include(l => l.Item)
            .Include(l => l.Borrower)
            .Where(l => l.Item.OwnerId == userId)
            .ToListAsync();
    }

    public async Task<Loan?> GetLoanByIdAsync(int id)
    {
        return await dbContext.Loans
            .Include(l => l.Item)
            .ThenInclude(i => i.Owner)
            .Include(l => l.Borrower)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<(bool success, string? errorMessage, Loan? loan)> CreateLoanAsync(CreateLoanRequest request,
        int userId)
    {
        var item = await dbContext.Items
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == request.ItemId);

        if (item == null)
            return (false, "Item not found", null);

        if (!item.IsAvailable)
            return (false, "Item is not available for loan", null);

        if (item.OwnerId == userId)
            return (false, "You cannot borrow your own item", null);

        var loan = new Loan
        {
            ItemId = request.ItemId,
            BorrowerId = userId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Notes = request.Notes,
            Status = LoanStatus.Requested
        };

        item.IsAvailable = false;
        dbContext.Loans.Add(loan);
        await dbContext.SaveChangesAsync();

        var createdLoan = await GetLoanByIdAsync(loan.Id);
        return (true, null, createdLoan);
    }

    public async Task<(bool success, string? errorMessage)> UpdateLoanStatusAsync(int loanId, LoanStatus status,
        int userId)
    {
        var loan = await dbContext.Loans
            .Include(l => l.Item)
            .FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan == null)
            return (false, "Loan not found");

        if (loan.BorrowerId != userId && loan.Item.OwnerId != userId)
            return (false, "Unauthorized");

        if (!IsStatusChangeAllowed(loan.Status, status, loan.BorrowerId == userId))
            return (false, "Status change not allowed");

        loan.Status = status;

        if (status == LoanStatus.Returned)
        {
            loan.ActualReturnDate = DateTime.UtcNow;
            loan.Item.IsAvailable = true;
        }

        await dbContext.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool success, string? errorMessage)> InitiateReturnAsync(int loanId, int userId)
    {
        var loan = await dbContext.Loans
            .Include(l => l.Item)
            .FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan == null)
            return (false, "Loan not found");

        if (loan.BorrowerId != userId)
            return (false, "Unauthorized");

        if (loan.Status != LoanStatus.Active)
            return (false, "Lånet måste vara aktivt för att kunna återlämnas");

        loan.Status = LoanStatus.ReturnInitiated;
        await dbContext.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool success, string? errorMessage)> ConfirmReturnAsync(int loanId, int userId)
    {
        var loan = await dbContext.Loans
            .Include(l => l.Item)
            .FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan == null)
            return (false, "Loan not found");

        if (loan.Item.OwnerId != userId)
            return (false, "Unauthorized");

        if (loan.Status != LoanStatus.ReturnInitiated)
            return (false, "Låntagaren måste först markera föremålet som återlämnat");

        loan.Status = LoanStatus.Returned;
        loan.ActualReturnDate = DateTime.UtcNow;
        loan.Item.IsAvailable = true;

        await dbContext.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool success, string? errorMessage)> ApproveRequestAsync(int loanId, int userId)
    {
        var loan = await dbContext.Loans
            .Include(l => l.Item)
            .FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan == null)
            return (false, "Loan not found");

        if (loan.Item.OwnerId != userId)
            return (false, "Unauthorized");

        if (loan.Status != LoanStatus.Requested)
            return (false, "Endast förfrågningar kan godkännas");

        loan.Status = LoanStatus.Approved;
        await dbContext.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool success, string? errorMessage)> RejectRequestAsync(int loanId, int userId)
    {
        var loan = await dbContext.Loans
            .Include(l => l.Item)
            .FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan == null)
            return (false, "Loan not found");

        if (loan.Item.OwnerId != userId)
            return (false, "Unauthorized");

        if (loan.Status != LoanStatus.Requested)
            return (false, "Endast förfrågningar kan avslås");

        loan.Status = LoanStatus.Rejected;
        loan.Item.IsAvailable = true;

        await dbContext.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool success, string? errorMessage)> ActivateLoanAsync(int loanId, int userId)
    {
        var loan = await dbContext.Loans
            .Include(l => l.Item)
            .FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan == null)
            return (false, "Loan not found");

        if (loan.Item.OwnerId != userId)
            return (false, "Unauthorized");

        if (loan.Status != LoanStatus.Approved)
            return (false, "Endast godkända lån kan aktiveras");

        loan.Status = LoanStatus.Active;
        await dbContext.SaveChangesAsync();
        return (true, null);
    }
}