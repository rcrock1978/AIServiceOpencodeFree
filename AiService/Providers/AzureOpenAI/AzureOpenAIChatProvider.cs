using System.Net.Http.Json;
using AiService.Configuration;
using AiService.Models;
using Microsoft.Extensions.Options;

namespace AiService.Providers.AzureOpenAI;

public class AzureOpenAIChatProvider : IChatProvider
{
    private readonly HttpClient _httpClient;
    private readonly AzureOpenAIOptions _options;

    public AzureOpenAIChatProvider(HttpClient httpClient, IOptions<AzureOpenAIOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.DefaultRequestHeaders.Add("api-key", _options.ApiKey);
    }

    public string ProviderName => "AzureOpenAI";

    public async Task<string> GenerateChatResponseAsync(string systemPrompt, string userMessage, List<Message>? history = null)
    {
        var messages = new List<AzureMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };

        if (history is not null)
        {
            foreach (var msg in history)
            {
                messages.Add(new AzureMessage { Role = msg.Role, Content = msg.Content });
            }
        }

        messages.Add(new AzureMessage { Role = "user", Content = userMessage });

        var request = new
        {
            messages,
            max_tokens = 1024,
            temperature = 0.7
        };

        var url = $"{_options.Endpoint}/openai/deployments/{_options.ChatDeployment}/chat/completions?api-version={_options.ApiVersion}";
        var response = await _httpClient.PostAsJsonAsync(url, request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AzureChatResponse>();
        return result?.Choices?.Length > 0 ? result.Choices[0].Message?.Content ?? string.Empty : string.Empty;
    }

    private sealed record AzureMessage
    {
        public string Role { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
    }

    private sealed record AzureChatResponse(AzureChoice[] Choices);
    private sealed record AzureChoice(AzureMessage? Message);
}
