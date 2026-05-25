using AiService.Models;
using AiService.Services;
using MediatR;

namespace AiService.Features.Search;

public class HybridSearchHandler(ISearchService searchService) : IRequestHandler<HybridSearchQuery, IEnumerable<Product>>
{
    public async Task<IEnumerable<Product>> Handle(HybridSearchQuery request, CancellationToken cancellationToken)
    {
        return await searchService.HybridSearchAsync(request.Query, request.Limit);
    }
}
