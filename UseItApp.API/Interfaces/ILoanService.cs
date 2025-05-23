using UseItApp.API.Models;
using UseItApp.Domain.Enums;
using UseItApp.Domain.Models;

namespace UseItApp.API.Interfaces;

public interface ILoanService
{
    Task CheckOverdueLoans();
    bool IsStatusChangeAllowed(LoanStatus currentStatus, LoanStatus newStatus, bool isBorrower);
    
    Task<IEnumerable<Loan>> GetLoansForUserAsync(int userId);
    Task<IEnumerable<Loan>> GetUserLoansAsync(int userId);
    Task<IEnumerable<Loan>> GetOwnerLoansAsync(int userId);
    Task<Loan?> GetLoanByIdAsync(int id);
    Task<(bool success, string? errorMessage, Loan? loan)> CreateLoanAsync(CreateLoanRequest request, int userId);
    Task<(bool success, string? errorMessage)> UpdateLoanStatusAsync(int loanId, LoanStatus status, int userId);
    Task<(bool success, string? errorMessage)> InitiateReturnAsync(int loanId, int userId);
    Task<(bool success, string? errorMessage)> ConfirmReturnAsync(int loanId, int userId);
    Task<(bool success, string? errorMessage)> ApproveRequestAsync(int loanId, int userId);
    Task<(bool success, string? errorMessage)> RejectRequestAsync(int loanId, int userId);
    Task<(bool success, string? errorMessage)> ActivateLoanAsync(int loanId, int userId);
}