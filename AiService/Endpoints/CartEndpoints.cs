using AiService.Contracts;
using AiService.Features.Cart;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiService.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this WebApplication app)
    {
        app.MapPost("/cart/add", async (AddToCartRequest request, [FromServices] IMediator mediator) =>
        {
            var command = new AddToCartCommand(request.ProductId, request.Quantity);
            var response = await mediator.Send(command);
            return Results.Ok(response);
        });

        app.MapGet("/cart", async ([FromServices] IMediator mediator) =>
        {
            var query = new GetCartQuery();
            var response = await mediator.Send(query);
            return Results.Ok(response);
        });

        app.MapDelete("/cart/remove", async ([AsParameters] RemoveFromCartRequest request, [FromServices] IMediator mediator) =>
        {
            var command = new RemoveFromCartCommand(request.ProductId);
            await mediator.Send(command);
            return Results.Ok();
        });

        app.MapPost("/cart/checkout", async (CheckoutRequest request, [FromServices] IMediator mediator) =>
        {
            var command = new CheckoutCommand(request.ShippingAddress);
            var response = await mediator.Send(command);
            return Results.Ok(response);
        });
    }
}
