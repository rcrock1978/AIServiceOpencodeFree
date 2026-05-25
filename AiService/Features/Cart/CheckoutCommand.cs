using AiService.Contracts;
using MediatR;

namespace AiService.Features.Cart;

public record CheckoutCommand(string ShippingAddress) : IRequest<CheckoutResponse>;
