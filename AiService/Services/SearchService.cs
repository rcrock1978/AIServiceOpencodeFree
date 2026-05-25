using AiService.Models;
using AiService.Providers;
using Dapper;
using Npgsql;

namespace AiService.Services;

public class SearchService : ISearchService
{
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly string _connectionString;

    private static readonly HashSet<string> ProductKeywords =
    [
        "shirt", "shoes", "dress", "pants", "jacket", "hat", "socks", "bag",
        "laptop", "phone", "tablet", "headphones", "keyboard", "mouse", "monitor",
        "book", "toy", "game", "food", "drink", "snack", "oil", "filter",
        "nike", "adidas", "apple", "samsung", "sony", "bose", "dell", "hp",
        "electronics", "clothing", "accessories", "sports", "home", "kitchen"
    ];

    public SearchService(IEmbeddingProvider embeddingProvider, IConfiguration configuration)
    {
        _embeddingProvider = embeddingProvider;
        _connectionString = configuration.GetConnectionString("PgVector")
            ?? throw new InvalidOperationException("Connection string 'PgVector' not found.");
    }

    public async Task<IEnumerable<Product>> KeywordSearchAsync(string query, int page = 1, int pageSize = 20)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        var offset = (page - 1) * pageSize;
        var pattern = $"%{query}%";

        return await conn.QueryAsync<Product>(
            """
            SELECT id, name, description, price, brand, type, image_url AS ImageUrl,
                   rating, stock, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products
            WHERE name ILIKE @pattern OR description ILIKE @pattern
            ORDER BY
                CASE WHEN name ILIKE @exact THEN 0
                     WHEN name ILIKE @startsWith THEN 1
                     ELSE 2
                END,
                rating DESC, name
            LIMIT @limit OFFSET @offset
            """,
            new { pattern, exact = query, startsWith = $"{query}%", limit = pageSize, offset });
    }

    public async Task<IEnumerable<Product>> VectorSearchAsync(string query, int limit = 10)
    {
        var embedding = await _embeddingProvider.GenerateEmbeddingAsync(query);
        var dimensions = _embeddingProvider.Dimensions;
        var embeddingColumn = dimensions == 768 ? "embedding_768" : "embedding_1536";
        var vectorStr = "[" + string.Join(",", embedding) + "]";

        using var conn = new NpgsqlConnection(_connectionString);
        return await conn.QueryAsync<Product>(
            $"""
            SELECT id, name, description, price, brand, type, image_url AS ImageUrl,
                   rating, stock, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products
            WHERE {embeddingColumn} IS NOT NULL
            ORDER BY {embeddingColumn} <=> @target::vector({dimensions})
            LIMIT @limit
            """,
            new { target = vectorStr, limit });
    }

    public async Task<IEnumerable<Product>> HybridSearchAsync(string query, int limit = 10)
    {
        var vectorTask = VectorSearchAsync(query, limit);
        var keywordTask = KeywordSearchAsync(query, 1, limit);

        await Task.WhenAll(vectorTask, keywordTask);

        var vectorResults = await vectorTask;
        var keywordResults = await keywordTask;

        var seen = new HashSet<Guid>();
        var merged = new List<Product>();

        foreach (var product in vectorResults)
        {
            if (seen.Add(product.Id))
                merged.Add(product);
        }

        foreach (var product in keywordResults)
        {
            if (seen.Add(product.Id) && merged.Count < limit)
                merged.Add(product);
        }

        return merged.Take(limit);
    }

    public async Task<(IEnumerable<Product> Results, bool UsedSemantic)> SmartSearchAsync(string query, int limit = 10)
    {
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var isShort = words.Length < 4;
        var hasProductKeyword = words.Any(w => ProductKeywords.Contains(w.ToLowerInvariant()));

        if (isShort && hasProductKeyword)
        {
            var results = await KeywordSearchAsync(query, 1, limit);
            return (results, false);
        }

        var hybridResults = await HybridSearchAsync(query, limit);
        return (hybridResults, true);
    }
}
