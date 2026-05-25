using AiService.Contracts;
using MediatR;

namespace AiService.Features.Embeddings;

public record SeedEmbeddingsCommand : IRequest<SeedEmbeddingsResponse>;
