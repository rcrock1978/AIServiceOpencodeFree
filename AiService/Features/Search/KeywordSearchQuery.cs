using AiService.Models;
using MediatR;

namespace AiService.Features.Search;

public record KeywordSearchQuery(string Query, int Page = 1, int PageSize = 20) : IRequest<IEnumerable<Product>>;
