using MediatR;

namespace AiService.Features.Cart;

public record RemoveFromCartCommand(Guid ProductId) : IRequest;
