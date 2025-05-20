using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UseItApp.API.Controllers;
using UseItApp.API.Interfaces;
using UseItApp.API.Models;
using UseItApp.Domain.Models;

namespace UseItApp.Tests.UnitTests;

public class LoansControllerTests
{
    private LoansController SetupControllerWithUser(ILoanService service, int userId)
    {
        var controller = new LoansController(service);
        
        // Setup ClaimsPrincipal
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
        
        return controller;
    }
    
    [Fact]
    public async Task GetLoans_ReturnsUserLoans()
    {
        // Arrange
        var userId = 1;
        var expectedLoans = new List<Loan>
        {
            new Loan { Id = 1, BorrowerId = userId, Item = new Item { OwnerId = 2 } },
            new Loan { Id = 2, BorrowerId = 2, Item = new Item { OwnerId = userId } }
        };
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.GetLoansForUserAsync(userId))
            .ReturnsAsync(expectedLoans);
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.GetLoans();
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var loans = Assert.IsAssignableFrom<IEnumerable<Loan>>(okResult.Value);
        Assert.Equal(2, loans.Count());
    }
    
    [Fact]
    public async Task GetUserLoans_ReturnsBorrowedLoans()
    {
        // Arrange
        var userId = 1;
        var expectedLoans = new List<Loan>
        {
            new Loan { Id = 1, BorrowerId = userId, Item = new Item { OwnerId = 2 } },
            new Loan { Id = 2, BorrowerId = userId, Item = new Item { OwnerId = 3 } }
        };
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.GetUserLoansAsync(userId))
            .ReturnsAsync(expectedLoans);
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.GetUserLoans();
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var loans = Assert.IsAssignableFrom<IEnumerable<Loan>>(okResult.Value);
        Assert.Equal(2, loans.Count());
    }
    
    [Fact]
    public async Task GetOwnerLoans_ReturnsOwnedItemLoans()
    {
        // Arrange
        var userId = 1;
        var expectedLoans = new List<Loan>
        {
            new Loan { Id = 1, BorrowerId = 2, Item = new Item { OwnerId = userId } },
            new Loan { Id = 2, BorrowerId = 3, Item = new Item { OwnerId = userId } }
        };
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.GetOwnerLoansAsync(userId))
            .ReturnsAsync(expectedLoans);
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.GetOwnerLoans();
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var loans = Assert.IsAssignableFrom<IEnumerable<Loan>>(okResult.Value);
        Assert.Equal(2, loans.Count());
    }
    
    [Fact]
    public async Task GetLoan_WithValidIdAndAccess_ReturnsLoan()
    {
        // Arrange
        var userId = 1;
        var loanId = 1;
        var loan = new Loan 
        { 
            Id = loanId, 
            BorrowerId = userId, 
            Item = new Item { OwnerId = 2 } 
        };
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.GetLoanByIdAsync(loanId))
            .ReturnsAsync(loan);
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.GetLoan(loanId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedLoan = Assert.IsType<Loan>(okResult.Value);
        Assert.Equal(loanId, returnedLoan.Id);
    }
    
    [Fact]
    public async Task GetLoan_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        var loanId = 999;
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.GetLoanByIdAsync(loanId))
            .ReturnsAsync((Loan)null);
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.GetLoan(loanId);
        
        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
    
    [Fact]
    public async Task GetLoan_WithoutAccess_ReturnsForbid()
    {
        // Arrange
        var userId = 1;
        var loanId = 1;
        // Loan som tillhör andra användare (inte borrower eller owner)
        var loan = new Loan 
        { 
            Id = loanId, 
            BorrowerId = 2, 
            Item = new Item { OwnerId = 3 } 
        };
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.GetLoanByIdAsync(loanId))
            .ReturnsAsync(loan);
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.GetLoan(loanId);
        
        // Assert
        Assert.IsType<ForbidResult>(result.Result);
    }
    
    [Fact]
    public async Task CreateLoan_WithValidRequest_ReturnsCreatedLoan()
    {
        // Arrange
        var userId = 1;
        var request = new CreateLoanRequest 
        { 
            ItemId = 1,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(7),
            Notes = "Test loan"
        };
        
        var createdLoan = new Loan 
        { 
            Id = 1, 
            ItemId = request.ItemId,
            BorrowerId = userId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Notes = request.Notes,
            Status = LoanStatus.Requested
        };
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.CreateLoanAsync(request, userId))
            .ReturnsAsync((true, null, createdLoan));
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.CreateLoan(request);
        
        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedLoan = Assert.IsType<Loan>(createdAtResult.Value);
        Assert.Equal(1, returnedLoan.Id);
        Assert.Equal(request.ItemId, returnedLoan.ItemId);
    }
    
    [Fact]
    public async Task CreateLoan_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var request = new CreateLoanRequest 
        { 
            ItemId = 1,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(7)
        };
        
        var errorMessage = "You cannot borrow your own item";
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.CreateLoanAsync(request, userId))
            .ReturnsAsync((false, errorMessage, null));
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.CreateLoan(request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(errorMessage, badRequestResult.Value);
    }
    
    [Fact]
    public async Task ApproveRequest_WithValidIdAndAccess_ReturnsNoContent()
    {
        // Arrange
        var userId = 1;
        var loanId = 1;
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.ApproveRequestAsync(loanId, userId))
            .ReturnsAsync((true, null));
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.ApproveRequest(loanId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task ApproveRequest_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        var loanId = 999;
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.ApproveRequestAsync(loanId, userId))
            .ReturnsAsync((false, "Loan not found"));
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.ApproveRequest(loanId);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task ApproveRequest_WithoutAccess_ReturnsForbid()
    {
        // Arrange
        var userId = 1;
        var loanId = 1;
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.ApproveRequestAsync(loanId, userId))
            .ReturnsAsync((false, "Unauthorized"));
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.ApproveRequest(loanId);
        
        // Assert
        Assert.IsType<ForbidResult>(result);
    }
    
    [Fact]
    public async Task ConfirmReturn_WithValidIdAndAccess_ReturnsNoContent()
    {
        // Arrange
        var userId = 1;
        var loanId = 1;
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.ConfirmReturnAsync(loanId, userId))
            .ReturnsAsync((true, null));
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.ConfirmReturn(loanId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task InitiateReturn_WithValidIdAndAccess_ReturnsNoContent()
    {
        // Arrange
        var userId = 1;
        var loanId = 1;
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.InitiateReturnAsync(loanId, userId))
            .ReturnsAsync((true, null));
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.InitiateReturn(loanId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task RejectRequest_WithValidIdAndAccess_ReturnsNoContent()
    {
        // Arrange
        var userId = 1;
        var loanId = 1;
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.RejectRequestAsync(loanId, userId))
            .ReturnsAsync((true, null));
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.RejectRequest(loanId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task ActivateLoan_WithValidIdAndAccess_ReturnsNoContent()
    {
        // Arrange
        var userId = 1;
        var loanId = 1;
        
        var mockService = new Mock<ILoanService>();
        mockService.Setup(service => service.ActivateLoanAsync(loanId, userId))
            .ReturnsAsync((true, null));
        
        var controller = SetupControllerWithUser(mockService.Object, userId);
        
        // Act
        var result = await controller.ActivateLoan(loanId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}