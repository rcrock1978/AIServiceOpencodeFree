using AiService.Models;
using MediatR;

namespace AiService.Features.Search;

public record VectorSearchQuery(string Query, int Limit = 10) : IRequest<IEnumerable<Product>>;
