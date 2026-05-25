using AiService.Models;
using Dapper;
using Npgsql;

namespace AiService.Repositories;

public class PgVectorRepository : IPgVectorRepository
{
    private readonly string _connectionString;

    public PgVectorRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private static string EmbeddingColumn(int length) => length switch
    {
        768 => "embedding_768",
        1536 => "embedding_1536",
        3072 => "embedding_3072",
        _ => throw new ArgumentException($"Unsupported embedding dimension: {length}")
    };

    public async Task<IEnumerable<Product>> VectorSearchAsync(float[] embedding, int limit = 10)
    {
        var column = EmbeddingColumn(embedding.Length);
        var sql = $"""
            SELECT id, name, description, price, brand, type, image_url AS ImageUrl, rating, stock, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products
            WHERE {column} IS NOT NULL
            ORDER BY {column} <=> @target::vector
            LIMIT @limit
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Product>(sql, new { target = embedding, limit });
    }

    public async Task<IEnumerable<Product>> KeywordSearchAsync(string query, int limit = 10)
    {
        const string sql = """
            SELECT id, name, description, price, brand, type, image_url AS ImageUrl, rating, stock, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products
            WHERE name ILIKE @pattern OR description ILIKE @pattern
            ORDER BY
                CASE WHEN name ILIKE @exact THEN 0
                     WHEN name ILIKE @pattern THEN 1
                     ELSE 2
                END,
                rating DESC
            LIMIT @limit
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Product>(sql, new
        {
            pattern = $"%{query}%",
            exact = query,
            limit
        });
    }

    public async Task<IEnumerable<Product>> HybridSearchAsync(
        float[] embedding, string query, int limit = 10, float vectorWeight = 0.5f)
    {
        var column = EmbeddingColumn(embedding.Length);
        var keywordWeight = 1.0f - vectorWeight;
        var sql = $"""
            SELECT id, name, description, price, brand, type, image_url AS ImageUrl, rating, stock, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM (
                SELECT id, name, description, price, brand, type, image_url, rating, stock, created_at, updated_at,
                       1 - ({column} <=> @target::vector) AS vector_score,
                       0 AS keyword_score
                FROM products
                WHERE {column} IS NOT NULL
                ORDER BY vector_score DESC
                LIMIT @limit

                UNION ALL

                SELECT id, name, description, price, brand, type, image_url, rating, stock, created_at, updated_at,
                       0 AS vector_score,
                       CASE WHEN name ILIKE @pattern THEN 0.7 ELSE 0 END +
                       CASE WHEN description ILIKE @pattern THEN 0.3 ELSE 0 END AS keyword_score
                FROM products
                WHERE name ILIKE @pattern OR description ILIKE @pattern
                LIMIT @limit
            ) combined
            ORDER BY (@vectorWeight * vector_score + @keywordWeight * keyword_score) DESC
            LIMIT @limit
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Product>(sql, new
        {
            target = embedding,
            pattern = $"%{query}%",
            limit,
            vectorWeight,
            keywordWeight
        });
    }

    public async Task UpsertEmbeddingAsync(Guid productId, float[] embedding)
    {
        var column = EmbeddingColumn(embedding.Length);
        var sql = $"""
            UPDATE products
            SET {column} = @embedding::vector, updated_at = NOW()
            WHERE id = @productId
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new { productId, embedding });
    }

    public async Task<IEnumerable<Product>> SearchWithContextAsync(
        string query, float[] embedding, int limit = 10)
    {
        var column = EmbeddingColumn(embedding.Length);
        var sql = $"""
            SELECT id, name, description, price, brand, type, image_url AS ImageUrl, rating, stock, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM (
                SELECT id, name, description, price, brand, type, image_url, rating, stock, created_at, updated_at,
                       1 - ({column} <=> @target::vector) AS score
                FROM products
                WHERE {column} IS NOT NULL
                  AND (name ILIKE @pattern OR description ILIKE @pattern)

                UNION

                SELECT id, name, description, price, brand, type, image_url, rating, stock, created_at, updated_at,
                       CASE WHEN name ILIKE @pattern THEN 0.8 ELSE 0.3 END AS score
                FROM products
                WHERE name ILIKE @pattern OR description ILIKE @pattern
            ) combined
            ORDER BY score DESC
            LIMIT @limit
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Product>(sql, new
        {
            target = embedding,
            pattern = $"%{query}%",
            limit
        });
    }

    public async Task<IEnumerable<Product>> SeedEmbeddingAsync(Guid productId, string name, string description)
    {
        const string sql = """
            SELECT id, name, description, price, brand, type, image_url AS ImageUrl, rating, stock, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products
            WHERE id = @productId
              AND embedding_768 IS NULL
              AND embedding_1536 IS NULL
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Product>(sql, new { productId });
    }
}
