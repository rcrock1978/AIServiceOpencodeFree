using AiService.Contracts;
using MediatR;

namespace AiService.Features.Chat;

public record AskCommand(string Message) : IRequest<ChatResponse>;
