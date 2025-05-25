using Microsoft.EntityFrameworkCore;
using UseItApp.API.Interfaces;
using UseItApp.API.Models;
using UseItApp.Data;
using UseItApp.Domain.Enums;
using UseItApp.Domain.Models;

namespace UseItApp.API.Services;

public class ItemService(ApplicationDbContext dbContext) : IItemService
{
    public async Task<IEnumerable<Item>> GetAvailableItemsAsync()
    {
        return await dbContext.Items
            .Include(i => i.Owner)
            .Where(i => i.IsAvailable)
            .ToListAsync();
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        return await dbContext.Items
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<(bool success, string? errorMessage, Item? item)> CreateItemAsync(CreateItemRequest request,
        int userId)
    {
        var item = new Item
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            ImageUrl = request.ImageUrl,
            IsAvailable = request.IsAvailable,
            OwnerId = userId
        };

        dbContext.Items.Add(item);
        await dbContext.SaveChangesAsync();
        
        var createdItem = await GetItemByIdAsync(item.Id);
        return (true, null, createdItem);
    }

    public async Task<(bool success, string? errorMessage)> UpdateItemAsync(int id, UpdateItemRequest request,
        int userId)
    {
        var item = await dbContext.Items.FindAsync(id);

        if (item == null)
            return (false, "Item not found");

        if (item.OwnerId != userId)
            return (false, "Unauthorized - you are not the owner of this item");

        item.Name = request.Name;
        item.Description = request.Description;
        item.Category = request.Category;
        item.ImageUrl = request.ImageUrl;
        item.IsAvailable = request.IsAvailable;

        try
        {
            await dbContext.SaveChangesAsync();
            return (true, null);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await dbContext.Items.AnyAsync(i => i.Id == id))
                return (false, "Item not found");
            throw;
        }
    }

    public async Task<(bool success, string? errorMessage)> DeleteItemAsync(int id, int userId)
    {
        var item = await dbContext.Items.FindAsync(id);

        if (item == null)
            return (false, "Item not found");

        if (item.OwnerId != userId)
            return (false, "Unauthorized - you are not the owner of this item");
        
        var hasActiveLoans = await dbContext.Loans
            .AnyAsync(l => l.ItemId == id &&
                           (l.Status == LoanStatus.Active ||
                            l.Status == LoanStatus.Approved ||
                            l.Status == LoanStatus.Requested));

        if (hasActiveLoans)
            return (false, "Cannot delete item with active loans");

        dbContext.Items.Remove(item);
        await dbContext.SaveChangesAsync();
        return (true, null);
    }

    public async Task<IEnumerable<Item>> GetUserItemsAsync(int userId)
    {
        return await dbContext.Items
            .Include(i => i.Owner)
            .Where(i => i.OwnerId == userId)
            .ToListAsync();
    }
}