using UseItApp.API.Models;
using UseItApp.Domain.Models;

namespace UseItApp.API.Interfaces;

public interface IItemService
{
    Task<IEnumerable<Item>> GetAvailableItemsAsync();
    Task<Item?> GetItemByIdAsync(int id);
    Task<(bool success, string? errorMessage, Item? item)> CreateItemAsync(CreateItemRequest request, int userId);
    Task<(bool success, string? errorMessage)> UpdateItemAsync(int id, UpdateItemRequest request, int userId);
    Task<(bool success, string? errorMessage)> DeleteItemAsync(int id, int userId);
    Task<IEnumerable<Item>> GetUserItemsAsync(int userId);
}