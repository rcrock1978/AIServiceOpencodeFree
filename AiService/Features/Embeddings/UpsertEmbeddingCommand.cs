using MediatR;

namespace AiService.Features.Embeddings;

public record UpsertEmbeddingCommand(Guid ProductId, string Text) : IRequest;
