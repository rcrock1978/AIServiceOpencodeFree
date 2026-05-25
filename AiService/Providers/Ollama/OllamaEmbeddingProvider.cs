using System.Net.Http.Json;
using AiService.Configuration;
using Microsoft.Extensions.Options;

namespace AiService.Providers.Ollama;

public class OllamaEmbeddingProvider : IEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaEmbeddingProvider(HttpClient httpClient, IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public int Dimensions => _options.Dimensions;
    public string ProviderName => "Ollama";

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var request = new { model = _options.EmbeddingModel, prompt = text };
        var response = await _httpClient.PostAsJsonAsync($"{_options.BaseUrl}/api/embeddings", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
        return result?.Embedding ?? [];
    }

    private sealed record OllamaEmbeddingResponse(float[] Embedding);
}
