using AiService.Models;

namespace AiService.Contracts;

public record AskRequest(string Message);

public record AskWithContextRequest(string Message, Guid ConversationId);

public record ChatResponse(string Reply, List<Product>? Products = null, Guid? ConversationId = null);
