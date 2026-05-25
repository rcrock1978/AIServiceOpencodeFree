using AiService.Contracts;
using AiService.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiService.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/login", async (LoginRequest request, [FromServices] IMediator mediator) =>
        {
            var command = new LoginCommand(request.Email, request.Password);
            var response = await mediator.Send(command);
            return string.IsNullOrEmpty(response.Token)
                ? Results.Unauthorized()
                : Results.Ok(response);
        });

        app.MapPost("/auth/register", async (RegisterRequest request, [FromServices] IMediator mediator) =>
        {
            var command = new RegisterCommand(request.Email, request.Username, request.Password);
            var response = await mediator.Send(command);
            return string.IsNullOrEmpty(response.Token)
                ? Results.BadRequest(new { Error = "Registration failed. Email or username may already be taken." })
                : Results.Ok(response);
        });
    }
}
