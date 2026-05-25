namespace AiService.Repositories;

using AiService.Models;

public interface IPgVectorRepository
{
    Task<IEnumerable<Product>> VectorSearchAsync(float[] embedding, int limit = 10);
    Task<IEnumerable<Product>> KeywordSearchAsync(string query, int limit = 10);
    Task<IEnumerable<Product>> HybridSearchAsync(float[] embedding, string query, int limit = 10, float vectorWeight = 0.5f);
    Task UpsertEmbeddingAsync(Guid productId, float[] embedding);
    Task<IEnumerable<Product>> SearchWithContextAsync(string query, float[] embedding, int limit = 10);
    Task<IEnumerable<Product>> SeedEmbeddingAsync(Guid productId, string name, string description);
}
