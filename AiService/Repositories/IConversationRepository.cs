using AiService.Models;

namespace AiService.Repositories;

public interface IConversationRepository
{
    Task<Conversation> CreateConversationAsync(Guid? userId, string title);
    Task<Conversation?> GetConversationAsync(Guid conversationId);
    Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId);
    Task<Message> AddMessageAsync(Guid conversationId, string role, string content);
    Task<IEnumerable<Message>> GetMessagesAsync(Guid conversationId, int limit = 20);
    Task UpdateConversationTitleAsync(Guid conversationId, string title);
}
