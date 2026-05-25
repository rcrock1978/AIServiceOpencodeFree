using AiService.Contracts;
using MediatR;

namespace AiService.Features.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
