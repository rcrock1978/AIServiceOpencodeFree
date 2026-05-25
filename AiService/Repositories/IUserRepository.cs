using AiService.Models;

namespace AiService.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(Guid id);
    Task<User> CreateAsync(string email, string username, string passwordHash);
}
