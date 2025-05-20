using Microsoft.EntityFrameworkCore;
using UseItApp.API.Models;
using UseItApp.API.Services;
using UseItApp.Data;
using UseItApp.Domain.Models;

namespace UseItApp.Tests.UnitTests;

public class LoanServiceTests
{
    private DbContextOptions<ApplicationDbContext> GetInMemoryDbOptions(string dbName)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }
    
    [Fact]
    public async Task CreateLoanAsync_WithValidRequest_CreateLoan()
    {
        // Arrange
        var options = GetInMemoryDbOptions("CreateLoanTest");
        var borrowerId = 1;
        var ownerId = 2;
        
        // Seed the database
        using (var context = new ApplicationDbContext(options))
        {
            var owner = new User { Id = ownerId, Username = "owner" };
            var borrower = new User { Id = borrowerId, Username = "borrower" };
            
            context.Users.Add(owner);
            context.Users.Add(borrower);
            
            var item = new Item 
            { 
                Id = 1, 
                Name = "Test Item", 
                OwnerId = ownerId,
                IsAvailable = true,
                Owner = owner
            };
            context.Items.Add(item);
            
            await context.SaveChangesAsync();
        }
        
        // Act & Assert
        using (var context = new ApplicationDbContext(options))
        {
            var service = new LoanService(context);
            
            var request = new CreateLoanRequest 
            { 
                ItemId = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                Notes = "Test loan"
            };
            
            var result = await service.CreateLoanAsync(request, borrowerId);
            
            // Assert
            Assert.True(result.success);
            Assert.Null(result.errorMessage);
            Assert.NotNull(result.loan);
            Assert.Equal(borrowerId, result.loan.BorrowerId);
            Assert.Equal(1, result.loan.ItemId);
            Assert.Equal(LoanStatus.Requested, result.loan.Status);
            
            // Verify item is no longer available
            var item = await context.Items.FindAsync(1);
            Assert.False(item.IsAvailable);
        }
    }
    
    [Fact]
    public async Task CreateLoanAsync_BorrowingOwnItem_ReturnsFalse()
    {
        // Arrange
        var options = GetInMemoryDbOptions("CreateLoanOwnItemTest");
        var userId = 1;
        
        // Seed the database
        using (var context = new ApplicationDbContext(options))
        {
            var owner = new User { Id = userId, Username = "owner" };
            context.Users.Add(owner);
            
            var item = new Item 
            { 
                Id = 1, 
                Name = "Test Item", 
                OwnerId = userId,
                IsAvailable = true,
                Owner = owner
            };
            context.Items.Add(item);
            
            await context.SaveChangesAsync();
        }
        
        // Act & Assert
        using (var context = new ApplicationDbContext(options))
        {
            var service = new LoanService(context);
            
            var request = new CreateLoanRequest 
            { 
                ItemId = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7)
            };
            
            var result = await service.CreateLoanAsync(request, userId);
            
            // Assert
            Assert.False(result.success);
            Assert.Equal("You cannot borrow your own item", result.errorMessage);
            Assert.Null(result.loan);
            
            // Verify item is still available
            var item = await context.Items.FindAsync(1);
            Assert.True(item.IsAvailable);
        }
    }
    
    [Fact]
    public async Task ApproveRequestAsync_WithValidRequest_ChangesStatus()
    {
        // Arrange
        var options = GetInMemoryDbOptions("ApproveRequestTest");
        var borrowerId = 1;
        var ownerId = 2;
        
        // Seed the database
        using (var context = new ApplicationDbContext(options))
        {
            var owner = new User { Id = ownerId, Username = "owner" };
            var borrower = new User { Id = borrowerId, Username = "borrower" };
            
            context.Users.Add(owner);
            context.Users.Add(borrower);
            
            var item = new Item 
            { 
                Id = 1, 
                Name = "Test Item", 
                OwnerId = ownerId,
                IsAvailable = false,
                Owner = owner
            };
            context.Items.Add(item);
            
            var loan = new Loan
            {
                Id = 1,
                ItemId = 1,
                Item = item,
                BorrowerId = borrowerId,
                Borrower = borrower,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                Status = LoanStatus.Requested
            };
            context.Loans.Add(loan);
            
            await context.SaveChangesAsync();
        }
        
        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var service = new LoanService(context);
            var result = await service.ApproveRequestAsync(1, ownerId);
            
            // Assert
            Assert.True(result.success);
            Assert.Null(result.errorMessage);
            
            // Verify loan status was updated
            var loan = await context.Loans.FindAsync(1);
            Assert.Equal(LoanStatus.Approved, loan.Status);
        }
    }
}