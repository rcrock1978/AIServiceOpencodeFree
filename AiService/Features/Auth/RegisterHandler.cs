using AiService.Contracts;
using AiService.Services;
using MediatR;

namespace AiService.Features.Auth;

public class RegisterHandler(IAuthService authService) : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await authService.RegisterAsync(request.Email, request.Username, request.Password);
    }
}
