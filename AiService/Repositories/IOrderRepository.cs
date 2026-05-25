using AiService.Models;

namespace AiService.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateOrderAsync(Guid userId, decimal totalAmount, string shippingAddress, List<CartItem> items);
    Task<Order?> GetOrderAsync(Guid orderId);
    Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId);
    Task UpdateOrderStatusAsync(Guid orderId, string status);
}
