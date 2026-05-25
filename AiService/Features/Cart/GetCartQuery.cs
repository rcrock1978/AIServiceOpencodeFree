using AiService.Contracts;
using MediatR;

namespace AiService.Features.Cart;

public record GetCartQuery : IRequest<CartResponse>;
