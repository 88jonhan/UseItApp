using Microsoft.EntityFrameworkCore;
using UseItApp.API.Models;
using UseItApp.API.Services;
using UseItApp.Data;
using UseItApp.Domain.Enums;
using UseItApp.Domain.Models;

namespace UseItApp.Tests.UnitTests;

public class ItemServiceTests
{
    private DbContextOptions<ApplicationDbContext> GetInMemoryDbOptions(string dbName)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }
    
    [Fact]
    public async Task GetAvailableItemsAsync_ReturnsOnlyAvailableItems()
    {
        // Arrange
        var options = GetInMemoryDbOptions("GetAvailableItemsTest");
        
        // Seed the database
        using (var context = new ApplicationDbContext(options))
        {
            var user = new User { Id = 1, Username = "testuser" };
            context.Users.Add(user);
            
            var items = new List<Item>
            {
                new Item { Id = 1, Name = "Available Item", IsAvailable = true, OwnerId = 1, Owner = user },
                new Item { Id = 2, Name = "Unavailable Item", IsAvailable = false, OwnerId = 1, Owner = user },
                new Item { Id = 3, Name = "Another Available Item", IsAvailable = true, OwnerId = 1, Owner = user }
            };
            
            context.Items.AddRange(items);
            await context.SaveChangesAsync();
        }
        
        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var service = new ItemService(context);
            var availableItems = await service.GetAvailableItemsAsync();
            
            // Assert
            Assert.Equal(2, availableItems.Count());
            Assert.DoesNotContain(availableItems, i => i.Name == "Unavailable Item");
            Assert.Contains(availableItems, i => i.Name == "Available Item");
            Assert.Contains(availableItems, i => i.Name == "Another Available Item");
        }
    }
    
    [Fact]
    public async Task DeleteItemAsync_WithActiveLoans_ReturnsFalse()
    {
        // Arrange
        var options = GetInMemoryDbOptions("DeleteItemWithLoansTest");
        var userId = 1;
        
        // Seed the database
        using (var context = new ApplicationDbContext(options))
        {
            var user = new User { Id = userId, Username = "owner" };
            var borrower = new User { Id = 2, Username = "borrower" };
            
            context.Users.AddRange(user, borrower);
            
            var item = new Item 
            { 
                Id = 1, 
                Name = "Test Item", 
                OwnerId = userId,
                IsAvailable = false
            };
            context.Items.Add(item);
            
            var loan = new Loan
            {
                Id = 1,
                ItemId = 1,
                BorrowerId = 2,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                Status = LoanStatus.Active
            };
            context.Loans.Add(loan);
            
            await context.SaveChangesAsync();
        }
        
        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var service = new ItemService(context);
            var result = await service.DeleteItemAsync(1, userId);
            
            // Assert
            Assert.False(result.success);
            Assert.Equal("Cannot delete item with active loans", result.errorMessage);
            
            // Verify item was not deleted
            var itemExists = await context.Items.AnyAsync(i => i.Id == 1);
            Assert.True(itemExists);
        }
    }
    
    [Fact]
    public async Task UpdateItemAsync_AsNonOwner_ReturnsFalse()
    {
        // Arrange
        var options = GetInMemoryDbOptions("UpdateItemNonOwnerTest");
        var ownerId = 1;
        var nonOwnerId = 2;
        
        // Seed the database
        using (var context = new ApplicationDbContext(options))
        {
            var owner = new User { Id = ownerId, Username = "owner" };
            var nonOwner = new User { Id = nonOwnerId, Username = "nonowner" };
            
            context.Users.AddRange(owner, nonOwner);
            
            var item = new Item 
            { 
                Id = 1, 
                Name = "Original Name", 
                Description = "Original Description",
                OwnerId = ownerId,
                IsAvailable = true
            };
            context.Items.Add(item);
            
            await context.SaveChangesAsync();
        }
        
        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var service = new ItemService(context);
            
            var updateRequest = new UpdateItemRequest
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Category = "Updated Category",
                IsAvailable = false
            };
            
            var result = await service.UpdateItemAsync(1, updateRequest, nonOwnerId);
            
            // Assert
            Assert.False(result.success);
            Assert.Equal("Unauthorized - you are not the owner of this item", result.errorMessage);
            
            // Verify item was not updated
            var item = await context.Items.FindAsync(1);
            Assert.Equal("Original Name", item.Name);
            Assert.Equal("Original Description", item.Description);
        }
    }
}