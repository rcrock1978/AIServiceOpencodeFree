using System.Security.Claims;
using AiService.Repositories;
using MediatR;

namespace AiService.Features.Cart;

public class RemoveFromCartHandler(ICartRepository cartRepository, IHttpContextAccessor httpContextAccessor) : IRequestHandler<RemoveFromCartCommand>
{
    public async Task Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var cart = await cartRepository.GetCartAsync(userId);
        if (cart != null)
        {
            await cartRepository.RemoveItemAsync(cart.Id, request.ProductId);
        }
    }

    private Guid GetUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}
