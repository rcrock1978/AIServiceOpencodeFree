using AiService.Contracts;
using AiService.Providers;
using MediatR;

namespace AiService.Features.Embeddings;

public class GenerateEmbeddingHandler(IEmbeddingProvider embeddingProvider) : IRequestHandler<GenerateEmbeddingCommand, GenerateEmbeddingResponse>
{
    public async Task<GenerateEmbeddingResponse> Handle(GenerateEmbeddingCommand request, CancellationToken cancellationToken)
    {
        var embedding = await embeddingProvider.GenerateEmbeddingAsync(request.Text);
        return new GenerateEmbeddingResponse([.. embedding], embeddingProvider.Dimensions);
    }
}
