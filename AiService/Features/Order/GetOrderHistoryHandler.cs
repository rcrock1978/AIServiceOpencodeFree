using System.Security.Claims;
using AiService.Contracts;
using AiService.Repositories;
using MediatR;

namespace AiService.Features.Order;

public class GetOrderHistoryHandler(IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetOrderHistoryQuery, OrderHistoryResponse>
{
    public async Task<OrderHistoryResponse> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var orders = await orderRepository.GetUserOrdersAsync(userId);
        return new OrderHistoryResponse([.. orders]);
    }

    private Guid GetUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}
