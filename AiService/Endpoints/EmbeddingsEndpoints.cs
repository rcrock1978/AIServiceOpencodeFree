using AiService.Contracts;
using AiService.Features.Embeddings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiService.Endpoints;

public static class EmbeddingsEndpoints
{
    public static void MapEmbeddingsEndpoints(this WebApplication app)
    {
        app.MapPost("/embeddings/generate", async (GenerateEmbeddingRequest request, [FromServices] IMediator mediator) =>
        {
            var command = new GenerateEmbeddingCommand(request.Text);
            var response = await mediator.Send(command);
            return Results.Ok(response);
        });

        app.MapPost("/embeddings/upsert", async (UpsertEmbeddingRequest request, [FromServices] IMediator mediator) =>
        {
            var command = new UpsertEmbeddingCommand(request.ProductId, request.Text);
            await mediator.Send(command);
            return Results.Ok();
        });

        app.MapPost("/embeddings/seed", async ([FromServices] IMediator mediator) =>
        {
            var command = new SeedEmbeddingsCommand();
            var response = await mediator.Send(command);
            return Results.Ok(response);
        });
    }
}
