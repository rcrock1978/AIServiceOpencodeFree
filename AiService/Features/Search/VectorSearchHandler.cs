using AiService.Models;
using AiService.Services;
using MediatR;

namespace AiService.Features.Search;

public class VectorSearchHandler(ISearchService searchService) : IRequestHandler<VectorSearchQuery, IEnumerable<Product>>
{
    public async Task<IEnumerable<Product>> Handle(VectorSearchQuery request, CancellationToken cancellationToken)
    {
        return await searchService.VectorSearchAsync(request.Query, request.Limit);
    }
}
