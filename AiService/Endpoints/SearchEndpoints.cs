using AiService.Contracts;
using AiService.Features.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiService.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this WebApplication app)
    {
        app.MapPost("/search/keyword", async (KeywordSearchRequest request, [FromServices] IMediator mediator) =>
        {
            var query = new KeywordSearchQuery(request.Query, request.Page, request.PageSize);
            var results = await mediator.Send(query);
            return Results.Ok(new SearchResponse([.. results], request.Query));
        });

        app.MapPost("/search/vector", async (VectorSearchRequest request, [FromServices] IMediator mediator) =>
        {
            var query = new VectorSearchQuery(request.Query, request.Limit);
            var results = await mediator.Send(query);
            return Results.Ok(new VectorSearchResponse([.. results], request.Query));
        });

        app.MapPost("/search/hybrid", async (HybridSearchRequest request, [FromServices] IMediator mediator) =>
        {
            var query = new HybridSearchQuery(request.Query, request.Limit);
            var results = await mediator.Send(query);
            return Results.Ok(new VectorSearchResponse([.. results], request.Query));
        });
    }
}
