using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AiService.Configuration;
using AiService.Contracts;
using AiService.Models;
using AiService.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace AiService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtOptions _jwtOptions;

    public AuthService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PgVector")
            ?? throw new InvalidOperationException("Connection string 'PgVector' not found.");
        _userRepository = new Repositories.UserRepository(connectionString);
        _jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration not found.");
    }

    public AuthService(IUserRepository userRepository, JwtOptions jwtOptions)
    {
        _userRepository = userRepository;
        _jwtOptions = jwtOptions;
    }

    public async Task<AuthResponse> RegisterAsync(string email, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return new AuthResponse(string.Empty);

        var existingEmail = await _userRepository.GetByEmailAsync(email);
        if (existingEmail != null)
            return new AuthResponse(string.Empty);

        var existingUsername = await _userRepository.GetByUsernameAsync(username);
        if (existingUsername != null)
            return new AuthResponse(string.Empty);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        var user = await _userRepository.CreateAsync(email, username, passwordHash);

        var token = GenerateToken(user);
        return new AuthResponse(token, user);
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return new AuthResponse(string.Empty);

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return new AuthResponse(string.Empty);

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return new AuthResponse(string.Empty);

        var token = GenerateToken(user);
        return new AuthResponse(token, user);
    }

    public Guid? ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                return userId;

            return null;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
