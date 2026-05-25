using System.Net.Http.Json;
using AiService.Configuration;
using AiService.Models;
using Microsoft.Extensions.Options;

namespace AiService.Providers.Ollama;

public class OllamaChatProvider : IChatProvider
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaChatProvider(HttpClient httpClient, IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public string ProviderName => "Ollama";

    public async Task<string> GenerateChatResponseAsync(string systemPrompt, string userMessage, List<Message>? history = null)
    {
        var messages = new List<OllamaMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };

        if (history is not null)
        {
            foreach (var msg in history)
            {
                messages.Add(new OllamaMessage { Role = msg.Role, Content = msg.Content });
            }
        }

        messages.Add(new OllamaMessage { Role = "user", Content = userMessage });

        var request = new
        {
            model = _options.ChatModel,
            messages,
            stream = false,
            options = new
            {
                temperature = _options.Temperature,
                num_predict = _options.MaxTokens
            }
        };

        var response = await _httpClient.PostAsJsonAsync($"{_options.BaseUrl}/api/chat", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();
        return result?.Message?.Content ?? string.Empty;
    }

    private sealed record OllamaMessage
    {
        public string Role { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
    }

    private sealed record OllamaChatResponse(OllamaMessage? Message);
}
