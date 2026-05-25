namespace AiService.Contracts;

public record GenerateEmbeddingRequest(string Text);

public record GenerateEmbeddingResponse(List<float> Embedding, int Dimensions);

public record UpsertEmbeddingRequest(Guid ProductId, string Text);

public record SeedEmbeddingsResponse(int Count, string Message);
