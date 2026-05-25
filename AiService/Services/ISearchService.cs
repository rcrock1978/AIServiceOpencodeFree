using AiService.Models;

namespace AiService.Services;

public interface ISearchService
{
    Task<IEnumerable<Product>> KeywordSearchAsync(string query, int page = 1, int pageSize = 20);
    Task<IEnumerable<Product>> VectorSearchAsync(string query, int limit = 10);
    Task<IEnumerable<Product>> HybridSearchAsync(string query, int limit = 10);
    Task<(IEnumerable<Product> Results, bool UsedSemantic)> SmartSearchAsync(string query, int limit = 10);
}
