using Dapper;
using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;
using Npgsql;

namespace KnowledgeAi.Infrastructure.Persistence.Chat;

public sealed class ChatRepository : IChatRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ChatRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<ChatSession> GetOrCreateSessionAsync(Guid? sessionId, Guid userId, CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        if (sessionId is { } id)
        {
            const string selectSql = "select id, user_id, created_at from chat_sessions where id = @id;";
            var existing = await connection.QuerySingleOrDefaultAsync<ChatSessionRow>(
                new CommandDefinition(selectSql, new { id }, cancellationToken: cancellationToken));

            if (existing is not null)
            {
                return existing.ToEntity();
            }
        }

        const string insertSql = """
            insert into chat_sessions (id, user_id, created_at)
            values (@Id, @UserId, @CreatedAt)
            returning id, user_id, created_at;
            """;

        var row = await connection.QuerySingleAsync<ChatSessionRow>(new CommandDefinition(insertSql, new
        {
            Id = sessionId ?? Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken: cancellationToken));

        return row.ToEntity();
    }

    public async Task SaveMessageAsync(ChatMessage message, CancellationToken cancellationToken)
    {
        const string sql = """
            insert into chat_messages (id, chat_session_id, question, answer, domain, evidence_status, confidence, created_at)
            values (@Id, @ChatSessionId, @Question, @Answer, @Domain, @EvidenceStatus, @Confidence, @CreatedAt);
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            message.Id,
            message.ChatSessionId,
            message.Question,
            message.Answer,
            Domain = message.Domain.ToString(),
            EvidenceStatus = message.EvidenceStatus.ToString(),
            message.Confidence,
            CreatedAt = message.CreatedAt.UtcDateTime,
        }, cancellationToken: cancellationToken));
    }

    private sealed class ChatSessionRow
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public DateTime CreatedAt { get; init; }

        public ChatSession ToEntity() => new()
        {
            Id = Id,
            UserId = UserId,
            CreatedAt = new DateTimeOffset(CreatedAt, TimeSpan.Zero),
        };
    }
}
