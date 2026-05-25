using System.Net.Http.Json;
using AiService.Configuration;
using Microsoft.Extensions.Options;

namespace AiService.Providers.AzureOpenAI;

public class AzureOpenAIEmbeddingProvider : IEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly AzureOpenAIOptions _options;

    public AzureOpenAIEmbeddingProvider(HttpClient httpClient, IOptions<AzureOpenAIOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.DefaultRequestHeaders.Add("api-key", _options.ApiKey);
    }

    public int Dimensions => _options.Dimensions;
    public string ProviderName => "AzureOpenAI";

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var request = new { input = text };
        var url = $"{_options.Endpoint}/openai/deployments/{_options.EmbeddingDeployment}/embeddings?api-version={_options.ApiVersion}";
        var response = await _httpClient.PostAsJsonAsync(url, request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AzureEmbeddingResponse>();
        return result?.Data?.Length > 0 ? result.Data[0].Embedding : [];
    }

    private sealed record AzureEmbeddingResponse(AzureEmbeddingDataItem[] Data);
    private sealed record AzureEmbeddingDataItem(float[] Embedding);
}
