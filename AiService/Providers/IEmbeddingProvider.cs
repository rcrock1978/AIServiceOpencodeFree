namespace AiService.Providers;

public interface IEmbeddingProvider
{
    Task<float[]> GenerateEmbeddingAsync(string text);
    int Dimensions { get; }
    string ProviderName { get; }
}
