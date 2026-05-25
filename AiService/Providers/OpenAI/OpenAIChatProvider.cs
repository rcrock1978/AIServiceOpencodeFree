using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiService.Configuration;
using AiService.Models;
using Microsoft.Extensions.Options;

namespace AiService.Providers.OpenAI;

public class OpenAIChatProvider : IChatProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIOptions _options;

    public OpenAIChatProvider(HttpClient httpClient, IOptions<OpenAIOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public string ProviderName => "OpenAI";

    public async Task<string> GenerateChatResponseAsync(string systemPrompt, string userMessage, List<Message>? history = null)
    {
        var messages = new List<OpenAIMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };

        if (history is not null)
        {
            foreach (var msg in history)
            {
                messages.Add(new OpenAIMessage { Role = msg.Role, Content = msg.Content });
            }
        }

        messages.Add(new OpenAIMessage { Role = "user", Content = userMessage });

        var request = new
        {
            model = _options.ChatModel,
            messages,
            max_tokens = 1024,
            temperature = 0.7
        };

        var response = await _httpClient.PostAsJsonAsync($"{_options.BaseUrl}/chat/completions", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIChatResponse>();
        return result?.Choices?.Length > 0 ? result.Choices[0].Message?.Content ?? string.Empty : string.Empty;
    }

    private sealed record OpenAIMessage
    {
        public string Role { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
    }

    private sealed record OpenAIChatResponse(OpenAIChoice[] Choices);
    private sealed record OpenAIChoice(OpenAIMessage? Message);
}
