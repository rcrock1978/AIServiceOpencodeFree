using AiService.Models;

namespace AiService.Contracts;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Username, string Password);

public record AuthResponse(string Token, User? User = null);
