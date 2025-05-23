using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UseItApp.API.Controllers;
using UseItApp.API.Services;
using UseItApp.Data;
using UseItApp.Domain.Enums;
using UseItApp.Domain.Models;

namespace UseItApp.Tests.IntegrationTests;

public class SimplifiedIntegrationTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public SimplifiedIntegrationTests()
    {
        // Skapa en unik databasnamn för varje testkörning för att undvika konflikter
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        // Seeda databasen med testdata
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        using var context = new ApplicationDbContext(_options);
        
        // Rensa databasen först (för säkerhets skull)
        context.Items.RemoveRange(context.Items);
        context.Loans.RemoveRange(context.Loans);
        context.Users.RemoveRange(context.Users);
        context.SaveChanges();

        // Lägg till testanvändare
        var user1 = new User
        {
            Id = 1,
            Username = "testuser1",
            Email = "test1@example.com",
            FirstName = "Test",
            LastName = "User1",
            PasswordHash = "hashedpassword123"
        };

        var user2 = new User
        {
            Id = 2,
            Username = "testuser2",
            Email = "test2@example.com",
            FirstName = "Test",
            LastName = "User2",
            PasswordHash = "hashedpassword456"
        };

        context.Users.AddRange(user1, user2);

        // Lägg till testföremål
        var item1 = new Item
        {
            Id = 1,
            Name = "Test Item 1",
            Description = "This is test item 1",
            Category = "Test",
            IsAvailable = true,
            OwnerId = 1,
            Owner = user1
        };

        var item2 = new Item
        {
            Id = 2,
            Name = "Test Item 2",
            Description = "This is test item 2",
            Category = "Test",
            IsAvailable = false,
            OwnerId = 2,
            Owner = user2
        };

        context.Items.AddRange(item1, item2);

        // Lägg till testlån
        var loan = new Loan
        {
            Id = 1,
            ItemId = 2,
            Item = item2,
            BorrowerId = 1,
            Borrower = user1,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = LoanStatus.Active
        };

        context.Loans.Add(loan);
        context.SaveChanges();
    }

    [Fact]
    public async Task GetItems_ReturnsOnlyAvailableItems()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var itemService = new ItemService(context);
        var controller = new ItemsController(itemService);

        // Act
        var result = await controller.GetItems();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<Item>>(okResult.Value);
        
        Assert.Single(items); // Bör bara vara ett tillgängligt objekt
        Assert.All(items, item => Assert.True(item.IsAvailable));
    }

    [Fact]
    public async Task GetItem_WithValidId_ReturnsItem()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var itemService = new ItemService(context);
        var controller = new ItemsController(itemService);
        var itemId = 1;

        // Act
        var result = await controller.GetItem(itemId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var item = Assert.IsType<Item>(okResult.Value);
        
        Assert.Equal(itemId, item.Id);
        Assert.Equal("Test Item 1", item.Name);
    }

    [Fact]
    public async Task GetItem_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var itemService = new ItemService(context);
        var controller = new ItemsController(itemService);
        var invalidItemId = 999;

        // Act
        var result = await controller.GetItem(invalidItemId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
    
    [Fact]
    public async Task GetUserItems_ReturnsOnlyUserOwnedItems()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var itemService = new ItemService(context);
        var controller = new ItemsController(itemService);
        
        // Simulera autentisering
        controller.ControllerContext = TestHelpers.GetControllerContextWithUser(1);

        // Act
        var result = await controller.GetUserItems();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<Item>>(okResult.Value);
        
        Assert.Single(items);
        Assert.All(items, item => Assert.Equal(1, item.OwnerId));
    }
    
    [Fact]
    public async Task CreateItem_WithValidRequest_CreatesAndReturnsItem()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var itemService = new ItemService(context);
        var controller = new ItemsController(itemService);
        
        // Simulera autentisering
        controller.ControllerContext = TestHelpers.GetControllerContextWithUser(1);
        
        var request = new API.Models.CreateItemRequest
        {
            Name = "New Test Item",
            Description = "This is a new test item",
            Category = "Test",
            IsAvailable = true,
            ImageUrl = "https://example.com/image.jpg" // Lägg till ImageUrl
        };

        // Act
        var result = await controller.CreateItem(request);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedItem = Assert.IsType<Item>(createdAtResult.Value);
        
        Assert.Equal(request.Name, returnedItem.Name);
        Assert.Equal(request.Description, returnedItem.Description);
        Assert.Equal(1, returnedItem.OwnerId);
        
        // Verifiera att objektet faktiskt skapades i databasen
        using var verificationContext = new ApplicationDbContext(_options);
        var itemInDb = await verificationContext.Items.FindAsync(returnedItem.Id);
        Assert.NotNull(itemInDb);
        Assert.Equal(request.Name, itemInDb!.Name);
    }
    
    [Fact]
    public async Task CreateLoan_WithValidRequest_CreatesAndReturnsLoan()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var loanService = new LoanService(context);
        var controller = new LoansController(loanService);
        
        // Simulera autentisering för user2 (som inte äger item1)
        controller.ControllerContext = TestHelpers.GetControllerContextWithUser(2);
        
        var request = new API.Models.CreateLoanRequest
        {
            ItemId = 1, // Item ägs av user1
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(7),
            Notes = "Test loan"
        };

        // Act
        var result = await controller.CreateLoan(request);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedLoan = Assert.IsType<Loan>(createdAtResult.Value);
        
        Assert.Equal(request.ItemId, returnedLoan.ItemId);
        Assert.Equal(2, returnedLoan.BorrowerId);
        Assert.Equal(LoanStatus.Requested, returnedLoan.Status);
        
        // Verifiera att lånet faktiskt skapades i databasen
        using var verificationContext = new ApplicationDbContext(_options);
        var loanInDb = await verificationContext.Loans.FindAsync(returnedLoan.Id);
        Assert.NotNull(loanInDb);
        
        // Verifiera att objektet är markerat som inte tillgängligt
        var item = await verificationContext.Items.FindAsync(request.ItemId);
        Assert.NotNull(item);
        Assert.False(item!.IsAvailable);
    }
    
    [Fact]
    public async Task GetLoans_ReturnsUserRelatedLoans()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var loanService = new LoanService(context);
        var controller = new LoansController(loanService);
        
        // Simulera autentisering för user1 (som har lånat item2)
        controller.ControllerContext = TestHelpers.GetControllerContextWithUser(1);

        // Act
        var result = await controller.GetLoans();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var loans = Assert.IsAssignableFrom<IEnumerable<Loan>>(okResult.Value);
        
        Assert.Single(loans);
        var loan = loans.First();
        Assert.Equal(1, loan.BorrowerId);
        Assert.Equal(2, loan.ItemId);
    }
}

// Hjälpklass för att skapa ControllerContext med autentiserad användare
public static class TestHelpers
{
    public static ControllerContext GetControllerContextWithUser(int userId)
    {
        var claims = new System.Collections.Generic.List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims);
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        
        return new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claimsPrincipal }
        };
    }
}