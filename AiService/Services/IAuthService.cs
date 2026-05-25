using AiService.Contracts;

namespace AiService.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(string email, string username, string password);
    Task<AuthResponse> LoginAsync(string email, string password);
    Guid? ValidateToken(string token);
}
