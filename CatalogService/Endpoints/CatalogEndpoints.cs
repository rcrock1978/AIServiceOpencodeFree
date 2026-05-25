using CatalogService.Repositories;
using CatalogService.Services;

namespace CatalogService.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/");

        group.MapGet("/products", async (
            IProductRepository repo,
            string? brand,
            string? type,
            string? search,
            string? sortBy,
            int page = 1,
            int pageSize = 20) =>
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var products = await repo.GetAllAsync(brand, type, search, sortBy, page, pageSize);
            var totalCount = await repo.GetTotalCountAsync(brand, type, search);

            return Results.Ok(new
            {
                Products = products,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        });

        group.MapGet("/products/{id:guid}", async (Guid id, IProductRepository repo) =>
        {
            var product = await repo.GetByIdAsync(id);
            return product is not null
                ? Results.Ok(product)
                : Results.Problem(
                    detail: $"Product with id '{id}' not found.",
                    statusCode: StatusCodes.Status404NotFound);
        });

        group.MapGet("/brands", async (IProductRepository repo) =>
        {
            var brands = await repo.GetBrandsAsync();
            return Results.Ok(brands);
        });

        group.MapGet("/types", async (IProductRepository repo) =>
        {
            var types = await repo.GetTypesAsync();
            return Results.Ok(types);
        });

        group.MapPost("/seed", async (SeedService seedService) =>
        {
            var count = await seedService.SeedAsync();
            return Results.Ok(new
            {
                Message = $"Successfully seeded {count} products.",
                Count = count
            });
        });
    }
}
