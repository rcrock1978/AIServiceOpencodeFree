using AiService.Models;
using Dapper;
using Npgsql;

namespace AiService.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly string _connectionString;

    public ConversationRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Conversation> CreateConversationAsync(Guid userId, string title)
    {
        const string sql = """
            INSERT INTO ai_conversations (id, user_id, title, is_active, created_at, updated_at)
            VALUES (gen_random_uuid(), @userId, @title, TRUE, NOW(), NOW())
            RETURNING id, user_id AS UserId, title, is_active AS IsActive, created_at AS CreatedAt, updated_at AS UpdatedAt
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstAsync<Conversation>(sql, new { userId, title });
    }

    public async Task<Conversation?> GetConversationAsync(Guid conversationId)
    {
        const string sql = """
            SELECT id, user_id AS UserId, title, is_active AS IsActive, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM ai_conversations
            WHERE id = @conversationId
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Conversation>(sql, new { conversationId });
    }

    public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId)
    {
        const string sql = """
            SELECT id, user_id AS UserId, title, is_active AS IsActive, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM ai_conversations
            WHERE user_id = @userId AND is_active = TRUE
            ORDER BY updated_at DESC
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Conversation>(sql, new { userId });
    }

    public async Task<Message> AddMessageAsync(Guid conversationId, string role, string content)
    {
        const string sql = """
            INSERT INTO ai_conversation_messages (id, conversation_id, role, content, created_at)
            VALUES (gen_random_uuid(), @conversationId, @role, @content, NOW())
            RETURNING id, conversation_id AS ConversationId, role, content, created_at AS CreatedAt
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstAsync<Message>(sql, new { conversationId, role, content });
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync(Guid conversationId, int limit = 20)
    {
        const string sql = """
            SELECT id, conversation_id AS ConversationId, role, content, created_at AS CreatedAt
            FROM ai_conversation_messages
            WHERE conversation_id = @conversationId
            ORDER BY created_at DESC
            LIMIT @limit
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Message>(sql, new { conversationId, limit });
    }

    public async Task UpdateConversationTitleAsync(Guid conversationId, string title)
    {
        const string sql = """
            UPDATE ai_conversations
            SET title = @title, updated_at = NOW()
            WHERE id = @conversationId
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new { conversationId, title });
    }
}
