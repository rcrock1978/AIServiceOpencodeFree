using System.Security.Claims;
using AiService.Contracts;
using AiService.Repositories;
using MediatR;

namespace AiService.Features.Cart;

public class AddToCartHandler(ICartRepository cartRepository, IHttpContextAccessor httpContextAccessor) : IRequestHandler<AddToCartCommand, CartResponse>
{
    public async Task<CartResponse> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var cart = await cartRepository.GetCartAsync(userId);
        if (cart == null)
        {
            cart = await cartRepository.CreateCartAsync(userId);
        }

        await cartRepository.AddItemAsync(cart.Id, request.ProductId, request.Quantity);

        cart = await cartRepository.GetCartAsync(userId);
        return new CartResponse(cart!);
    }

    private Guid GetUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}
