using AiService.Contracts;
using MediatR;

namespace AiService.Features.Chat;

public record AskWithContextCommand(string Message, Guid ConversationId) : IRequest<ChatResponse>;
