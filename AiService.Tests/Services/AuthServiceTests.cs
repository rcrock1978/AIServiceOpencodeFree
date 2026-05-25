using AiService.Configuration;
using AiService.Models;
using AiService.Repositories;
using AiService.Services;
using FluentAssertions;
using Moq;

namespace AiService.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo;
    private readonly JwtOptions _jwtOptions;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepo = new Mock<IUserRepository>();
        _jwtOptions = new JwtOptions
        {
            Key = "ThisIsASuperSecretTestKeyThatIsAtLeast32Characters!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpireMinutes = 60
        };
        _sut = new AuthService(_userRepo.Object, _jwtOptions);
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsValid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword", workFactor: 4)
        };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var result = await _sut.LoginAsync(user.Email, "correctpassword");

        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task LoginAsync_ReturnsEmptyToken_WhenPasswordInvalid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword", workFactor: 4)
        };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var result = await _sut.LoginAsync(user.Email, "wrongpassword");

        result.Token.Should().BeEmpty();
        result.User.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ReturnsEmptyToken_WhenUserNotFound()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync("unknown@example.com", "password");

        result.Token.Should().BeEmpty();
    }

    [Fact]
    public async Task LoginAsync_ReturnsEmptyToken_WhenEmailEmpty()
    {
        var result = await _sut.LoginAsync("", "password");

        result.Token.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_ReturnsToken_WhenNewUser()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new User
            {
                Id = Guid.NewGuid(),
                Email = "new@example.com",
                Username = "newuser",
                PasswordHash = "hash"
            });

        var result = await _sut.RegisterAsync("new@example.com", "newuser", "password123");

        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync_ReturnsEmptyToken_WhenEmailExists()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("existing@example.com"))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "existing@example.com" });

        var result = await _sut.RegisterAsync("existing@example.com", "newuser", "password123");

        result.Token.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_ReturnsEmptyToken_WhenUsernameExists()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByUsernameAsync("existinguser"))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Username = "existinguser" });

        var result = await _sut.RegisterAsync("new@example.com", "existinguser", "password123");

        result.Token.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_ReturnsEmptyToken_WhenInputsEmpty()
    {
        var result = await _sut.RegisterAsync("", "", "");

        result.Token.Should().BeEmpty();
    }

    [Fact]
    public void ValidateToken_ReturnsUserId_ForValidToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser"
        };
        var authService = new AuthService(_userRepo.Object, _jwtOptions);

        var token = GenerateTestToken(user);

        var userId = authService.ValidateToken(token);

        userId.Should().Be(user.Id);
    }

    [Fact]
    public void ValidateToken_ReturnsNull_ForInvalidToken()
    {
        var result = _sut.ValidateToken("invalid-token-string");

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ReturnsNull_ForExpiredToken()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Username = "testuser" };
        var expiredOptions = new JwtOptions
        {
            Key = _jwtOptions.Key,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            ExpireMinutes = -1
        };
        var authService = new AuthService(_userRepo.Object, expiredOptions);
        var token = GenerateTestToken(user, expiredOptions);

        var result = authService.ValidateToken(token);

        result.Should().BeNull();
    }

    private string GenerateTestToken(User user, JwtOptions? options = null)
    {
        var opts = options ?? _jwtOptions;
        var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(opts.Key));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: opts.Issuer,
            audience: opts.Audience,
            claims: new[]
            {
                new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.Name, user.Username)
            },
            expires: DateTime.UtcNow.AddMinutes(opts.ExpireMinutes),
            signingCredentials: credentials);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}
