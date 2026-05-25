using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiService.Configuration;
using Microsoft.Extensions.Options;

namespace AiService.Providers.OpenAI;

public class OpenAIEmbeddingProvider : IEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIOptions _options;

    public OpenAIEmbeddingProvider(HttpClient httpClient, IOptions<OpenAIOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public int Dimensions => _options.Dimensions;
    public string ProviderName => "OpenAI";

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var request = new { model = _options.EmbeddingModel, input = text };
        var response = await _httpClient.PostAsJsonAsync($"{_options.BaseUrl}/embeddings", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIEmbeddingResponse>();
        return result?.Data?.Length > 0 ? result.Data[0].Embedding : [];
    }

    private sealed record OpenAIEmbeddingResponse(OpenAIEmbeddingDataItem[] Data);
    private sealed record OpenAIEmbeddingDataItem(float[] Embedding);
}
