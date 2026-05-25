using AiService.Features.Order;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiService.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapGet("/orders", async ([FromServices] IMediator mediator) =>
        {
            var query = new GetOrderHistoryQuery();
            var response = await mediator.Send(query);
            return Results.Ok(response);
        });
    }
}
