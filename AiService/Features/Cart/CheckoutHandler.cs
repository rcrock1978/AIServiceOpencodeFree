using System.Security.Claims;
using AiService.Contracts;
using AiService.Repositories;
using MediatR;

namespace AiService.Features.Cart;

public class CheckoutHandler(ICartRepository cartRepository, IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor) : IRequestHandler<CheckoutCommand, CheckoutResponse>
{
    public async Task<CheckoutResponse> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var cart = await cartRepository.GetCartAsync(userId);
        if (cart == null || cart.Items.Count == 0)
        {
            return new CheckoutResponse(Guid.Empty, "Cart is empty", 0);
        }

        var totalAmount = cart.Items.Sum(i => (i.Product?.Price ?? 0) * i.Quantity);
        var order = await orderRepository.CreateOrderAsync(userId, totalAmount, request.ShippingAddress, cart.Items);
        await cartRepository.ClearCartAsync(cart.Id);

        return new CheckoutResponse(order.Id, "Order placed successfully", totalAmount);
    }

    private Guid GetUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}
