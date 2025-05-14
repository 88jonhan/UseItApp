using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UseItApp.API.Interfaces;
using UseItApp.API.Models;
using UseItApp.Domain.Models;

namespace UseItApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController(IItemService itemService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Item>>> GetItems()
    {
        var items = await itemService.GetAvailableItemsAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Item>> GetItem(int id)
    {
        var item = await itemService.GetItemByIdAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Item>> CreateItem(CreateItemRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, errorMessage, item) = await itemService.CreateItemAsync(request, userId);

        if (!success)
            return BadRequest(errorMessage);

        return CreatedAtAction(nameof(GetItem), new { id = item!.Id }, item);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateItem(int id, UpdateItemRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, errorMessage) = await itemService.UpdateItemAsync(id, request, userId);

        if (!success)
        {
            if (errorMessage == "Item not found")
                return NotFound();
            if (errorMessage!.StartsWith("Unauthorized"))
                return Forbid();
            return BadRequest(errorMessage);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, errorMessage) = await itemService.DeleteItemAsync(id, userId);

        if (!success)
        {
            if (errorMessage == "Item not found")
                return NotFound();
            if (errorMessage!.StartsWith("Unauthorized"))
                return Forbid();
            return BadRequest(errorMessage);
        }

        return NoContent();
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Item>>> GetUserItems()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var items = await itemService.GetUserItemsAsync(userId);
        return Ok(items);
    }
}