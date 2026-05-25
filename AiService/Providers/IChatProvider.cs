using AiService.Models;

namespace AiService.Providers;

public interface IChatProvider
{
    Task<string> GenerateChatResponseAsync(string systemPrompt, string userMessage, List<Message>? history = null);
    string ProviderName { get; }
}
