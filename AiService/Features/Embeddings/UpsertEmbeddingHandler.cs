using AiService.Providers;
using AiService.Repositories;
using MediatR;

namespace AiService.Features.Embeddings;

public class UpsertEmbeddingHandler(IEmbeddingProvider embeddingProvider, IPgVectorRepository pgVectorRepository) : IRequestHandler<UpsertEmbeddingCommand>
{
    public async Task Handle(UpsertEmbeddingCommand request, CancellationToken cancellationToken)
    {
        var embedding = await embeddingProvider.GenerateEmbeddingAsync(request.Text);
        await pgVectorRepository.UpsertEmbeddingAsync(request.ProductId, embedding);
    }
}
