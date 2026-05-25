using AiService.Models;

namespace AiService.Contracts;

public record AddToCartRequest(Guid ProductId, int Quantity = 1);

public record CartResponse(Cart Cart);

public record RemoveFromCartRequest(Guid ProductId);

public record CheckoutRequest(string ShippingAddress);

public record CheckoutResponse(Guid OrderId, string Status, decimal TotalAmount);
