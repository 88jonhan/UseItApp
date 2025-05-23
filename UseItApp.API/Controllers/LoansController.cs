using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UseItApp.API.Interfaces;
using UseItApp.API.Models;
using UseItApp.Domain.Enums;
using UseItApp.Domain.Models;

namespace UseItApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController(ILoanService loanService) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Loan>>> GetLoans()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var loans = await loanService.GetLoansForUserAsync(userId);
        return Ok(loans);
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Loan>>> GetUserLoans()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var loans = await loanService.GetUserLoansAsync(userId);
        return Ok(loans);
    }

    [HttpGet("owner")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Loan>>> GetOwnerLoans()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var loans = await loanService.GetOwnerLoansAsync(userId);
        return Ok(loans);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Loan>> GetLoan(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var loan = await loanService.GetLoanByIdAsync(id);

        if (loan == null)
            return NotFound();

        if (loan.BorrowerId != userId && loan.Item.OwnerId != userId)
            return Forbid();

        return Ok(loan);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Loan>> CreateLoan(CreateLoanRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, errorMessage, loan) = await loanService.CreateLoanAsync(request, userId);

        if (!success)
            return BadRequest(errorMessage);

        return CreatedAtAction(nameof(GetLoan), new { id = loan!.Id }, loan);
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateLoanStatus(int id, [FromBody] LoanStatus status)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, errorMessage) = await loanService.UpdateLoanStatusAsync(id, status, userId);

        if (!success)
        {
            if (errorMessage == "Loan not found")
                return NotFound();
            if (errorMessage == "Unauthorized")
                return Forbid();
            return BadRequest(errorMessage);
        }

        return NoContent();
    }

    [HttpPut("{id}/initiate-return")]
    [Authorize]
    public async Task<IActionResult> InitiateReturn(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, errorMessage) = await loanService.InitiateReturnAsync(id, userId);

        if (!success)
        {
            if (errorMessage == "Loan not found")
                return NotFound();
            if (errorMessage == "Unauthorized")
                return Forbid();
            return BadRequest(errorMessage);
        }

        return NoContent();
    }

    [HttpPut("{id}/confirm-return")]
    [Authorize]
    public async Task<IActionResult> ConfirmReturn(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, errorMessage) = await loanService.ConfirmReturnAsync(id, userId);

        if (!success)
        {
            if (errorMessage == "Loan not found")
                return NotFound();
            if (errorMessage == "Unauthorized")
                return Forbid();
            return BadRequest(errorMessage);
        }

        return NoContent();
    }

    [HttpPut("{id}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveRequest(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, errorMessage) = await loanService.ApproveRequestAsync(id, userId);

        if (!success)
        {
            if (errorMessage == "Loan not found")
                return NotFound();
            if (errorMessage == "Unauthorized")
                return Forbid();
            return BadRequest(errorMessage);
        }

        return NoContent();
    }

    [HttpPut("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectRequest(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, errorMessage) = await loanService.RejectRequestAsync(id, userId);

        if (!success)
        {
            if (errorMessage == "Loan not found")
                return NotFound();
            if (errorMessage == "Unauthorized")
                return Forbid();
            return BadRequest(errorMessage);
        }

        return NoContent();
    }

    [HttpPut("{id}/activate")]
    [Authorize]
    public async Task<IActionResult> ActivateLoan(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, errorMessage) = await loanService.ActivateLoanAsync(id, userId);

        if (!success)
        {
            if (errorMessage == "Loan not found")
                return NotFound();
            if (errorMessage == "Unauthorized")
                return Forbid();
            return BadRequest(errorMessage);
        }

        return NoContent();
    }
}