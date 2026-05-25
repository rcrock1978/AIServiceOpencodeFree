using AiService.Contracts;
using MediatR;

namespace AiService.Features.Cart;

public record AddToCartCommand(Guid ProductId, int Quantity = 1) : IRequest<CartResponse>;
