using AiService.Models;

namespace AiService.Contracts;

public record SearchRequest(string Query);

public record KeywordSearchRequest(string Query, int Page = 1, int PageSize = 20);

public record VectorSearchRequest(string Query, int Limit = 10);

public record HybridSearchRequest(string Query, int Limit = 10);

public record SearchResponse(List<Product> Results, string Query);

public record VectorSearchResponse(List<Product> Results, string Query);
