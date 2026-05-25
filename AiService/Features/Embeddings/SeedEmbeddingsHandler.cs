using System.Data;
using AiService.Contracts;
using AiService.Models;
using AiService.Providers;
using AiService.Repositories;
using Dapper;
using MediatR;
using Npgsql;

namespace AiService.Features.Embeddings;

public class SeedEmbeddingsHandler(IEmbeddingProvider embeddingProvider, IPgVectorRepository pgVectorRepository, IConfiguration configuration) : IRequestHandler<SeedEmbeddingsCommand, SeedEmbeddingsResponse>
{
    public async Task<SeedEmbeddingsResponse> Handle(SeedEmbeddingsCommand request, CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("PgVector")
            ?? throw new InvalidOperationException("Connection string 'PgVector' not found.");
        var dimensions = embeddingProvider.Dimensions;
        var embeddingColumn = dimensions == 768 ? "embedding_768" : "embedding_1536";

        using var conn = new NpgsqlConnection(connectionString);
        var products = await conn.QueryAsync<Product>(
            $"""
            SELECT id, name, description, price, brand, type, image_url AS ImageUrl,
                   rating, stock, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products
            WHERE {embeddingColumn} IS NULL
            """);

        var count = 0;
        foreach (var product in products)
        {
            var text = $"{product.Name} {product.Description ?? ""} {product.Brand ?? ""}";
            var embedding = await embeddingProvider.GenerateEmbeddingAsync(text);
            await pgVectorRepository.UpsertEmbeddingAsync(product.Id, embedding);
            count++;
        }

        return new SeedEmbeddingsResponse(count, $"Successfully seeded {count} product embeddings.");
    }
}
