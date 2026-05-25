using AiService.Models;
using Dapper;
using Npgsql;

namespace AiService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = """
            SELECT id, email, username, password_hash AS PasswordHash, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM users
            WHERE email = @email
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { email });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = """
            SELECT id, email, username, password_hash AS PasswordHash, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM users
            WHERE username = @username
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { username });
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT id, email, username, password_hash AS PasswordHash, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM users
            WHERE id = @id
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { id });
    }

    public async Task<User> CreateAsync(string email, string username, string passwordHash)
    {
        const string sql = """
            INSERT INTO users (id, email, username, password_hash, created_at, updated_at)
            VALUES (gen_random_uuid(), @email, @username, @passwordHash, NOW(), NOW())
            RETURNING id, email, username, password_hash AS PasswordHash, created_at AS CreatedAt, updated_at AS UpdatedAt
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstAsync<User>(sql, new { email, username, passwordHash });
    }
}
