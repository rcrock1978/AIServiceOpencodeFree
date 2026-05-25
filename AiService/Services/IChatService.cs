using AiService.Contracts;

namespace AiService.Services;

public interface IChatService
{
    Task<ChatResponse> AskAsync(string message);
    Task<ChatResponse> AskWithContextAsync(string message, Guid conversationId);
}
