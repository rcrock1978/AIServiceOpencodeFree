using AiService.Contracts;
using AiService.Features.Chat;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiService.Endpoints;

public static class ChatEndpoints
{
    public static void MapChatEndpoints(this WebApplication app)
    {
        app.MapPost("/chat/ask", async (AskRequest request, [FromServices] IMediator mediator) =>
        {
            var command = new AskCommand(request.Message);
            var response = await mediator.Send(command);
            return Results.Ok(response);
        });

        app.MapPost("/chat/ask/context", async (AskWithContextRequest request, [FromServices] IMediator mediator) =>
        {
            var command = new AskWithContextCommand(request.Message, request.ConversationId);
            var response = await mediator.Send(command);
            return Results.Ok(response);
        });
    }
}
