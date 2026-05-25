using AiService.Contracts;
using MediatR;

namespace AiService.Features.Auth;

public record RegisterCommand(string Email, string Username, string Password) : IRequest<AuthResponse>;
