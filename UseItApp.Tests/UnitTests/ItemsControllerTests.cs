using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UseItApp.API.Controllers;
using UseItApp.API.Interfaces;
using UseItApp.API.Models;
using UseItApp.Domain.Models;

namespace UseItApp.Tests.UnitTests;

public class ItemsControllerTests
{
    [Fact]
    public async Task GetItems_ReturnsAllAvailableItems()
    {
        // Arrange
        var mockService = new Mock<IItemService>();
        var expectedItems = new List<Item>
        {
            new Item { Id = 1, Name = "Item 1", IsAvailable = true },
            new Item { Id = 2, Name = "Item 2", IsAvailable = true }
        };
        
        mockService.Setup(service => service.GetAvailableItemsAsync())
            .ReturnsAsync(expectedItems);
        
        var controller = new ItemsController(mockService.Object);
        
        // Act
        var result = await controller.GetItems();
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<Item>>(okResult.Value);
        Assert.Equal(2, items.Count());
    }
    
    [Fact]
    public async Task GetItem_WithValidId_ReturnsItem()
    {
        // Arrange
        var itemId = 1;
        var expectedItem = new Item { Id = itemId, Name = "Test Item" };
        
        var mockService = new Mock<IItemService>();
        mockService.Setup(service => service.GetItemByIdAsync(itemId))
            .ReturnsAsync(expectedItem);
        
        var controller = new ItemsController(mockService.Object);
        
        // Act
        var result = await controller.GetItem(itemId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var item = Assert.IsType<Item>(okResult.Value);
        Assert.Equal(itemId, item.Id);
    }
    
    [Fact]
    public async Task GetItem_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var itemId = 999;
        
        var mockService = new Mock<IItemService>();
        mockService.Setup(service => service.GetItemByIdAsync(itemId))
            .ReturnsAsync((Item)null);
        
        var controller = new ItemsController(mockService.Object);
        
        // Act
        var result = await controller.GetItem(itemId);
        
        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
    
    [Fact]
    public async Task CreateItem_WithValidRequest_ReturnsCreatedItem()
    {
        // Arrange
        var userId = 1;
        var request = new CreateItemRequest 
        { 
            Name = "New Item", 
            Description = "Test Description",
            Category = "Test",
            IsAvailable = true
        };
        
        var createdItem = new Item 
        { 
            Id = 1, 
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            IsAvailable = request.IsAvailable,
            OwnerId = userId
        };
        
        var mockService = new Mock<IItemService>();
        mockService.Setup(service => service.CreateItemAsync(request, userId))
            .ReturnsAsync((true, null, createdItem));
        
        var controller = new ItemsController(mockService.Object);
        
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
        
        // Act
        var result = await controller.CreateItem(request);
        
        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedItem = Assert.IsType<Item>(createdAtResult.Value);
        Assert.Equal(createdItem.Id, returnedItem.Id);
        Assert.Equal(request.Name, returnedItem.Name);
    }
}