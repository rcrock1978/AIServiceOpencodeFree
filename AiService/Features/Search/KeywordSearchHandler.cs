using AiService.Models;
using AiService.Services;
using MediatR;

namespace AiService.Features.Search;

public class KeywordSearchHandler(ISearchService searchService) : IRequestHandler<KeywordSearchQuery, IEnumerable<Product>>
{
    public async Task<IEnumerable<Product>> Handle(KeywordSearchQuery request, CancellationToken cancellationToken)
    {
        return await searchService.KeywordSearchAsync(request.Query, request.Page, request.PageSize);
    }
}
