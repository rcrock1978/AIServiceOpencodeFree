using AiService.Contracts;
using AiService.Services;
using MediatR;

namespace AiService.Features.Chat;

public class AskWithContextCommandHandler(IChatService chatService) : IRequestHandler<AskWithContextCommand, ChatResponse>
{
    public async Task<ChatResponse> Handle(AskWithContextCommand request, CancellationToken cancellationToken)
    {
        return await chatService.AskWithContextAsync(request.Message, request.ConversationId);
    }
}
