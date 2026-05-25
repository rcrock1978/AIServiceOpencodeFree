using AiService.Contracts;
using AiService.Services;
using MediatR;

namespace AiService.Features.Chat;

public class AskCommandHandler(IChatService chatService) : IRequestHandler<AskCommand, ChatResponse>
{
    public async Task<ChatResponse> Handle(AskCommand request, CancellationToken cancellationToken)
    {
        return await chatService.AskAsync(request.Message);
    }
}
