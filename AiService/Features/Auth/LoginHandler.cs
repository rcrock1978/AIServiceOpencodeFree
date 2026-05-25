using AiService.Contracts;
using AiService.Services;
using MediatR;

namespace AiService.Features.Auth;

public class LoginHandler(IAuthService authService) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await authService.LoginAsync(request.Email, request.Password);
    }
}
