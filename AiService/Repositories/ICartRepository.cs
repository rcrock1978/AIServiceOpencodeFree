using AiService.Models;

namespace AiService.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetCartAsync(Guid userId);
    Task<Cart> CreateCartAsync(Guid userId);
    Task AddItemAsync(Guid cartId, Guid productId, int quantity);
    Task RemoveItemAsync(Guid cartId, Guid productId);
    Task UpdateItemQuantityAsync(Guid cartId, Guid productId, int quantity);
    Task ClearCartAsync(Guid cartId);
}
