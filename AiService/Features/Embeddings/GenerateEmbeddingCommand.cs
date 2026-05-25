using AiService.Contracts;
using MediatR;

namespace AiService.Features.Embeddings;

public record GenerateEmbeddingCommand(string Text) : IRequest<GenerateEmbeddingResponse>;
