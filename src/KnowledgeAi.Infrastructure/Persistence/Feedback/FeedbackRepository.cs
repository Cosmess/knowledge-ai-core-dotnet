using Dapper;
using KnowledgeAi.Application.Common.Interfaces;
using Npgsql;
using FeedbackEntity = KnowledgeAi.Domain.Entities.Feedback;

namespace KnowledgeAi.Infrastructure.Persistence.Feedback;

public sealed class FeedbackRepository : IFeedbackRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public FeedbackRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task SaveAsync(FeedbackEntity feedback, CancellationToken cancellationToken)
    {
        const string sql = """
            insert into feedbacks (id, chat_message_id, user_id, helpful, comment, created_at)
            values (@Id, @ChatMessageId, @UserId, @Helpful, @Comment, @CreatedAt);
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            feedback.Id,
            feedback.ChatMessageId,
            feedback.UserId,
            feedback.Helpful,
            feedback.Comment,
            CreatedAt = feedback.CreatedAt.UtcDateTime,
        }, cancellationToken: cancellationToken));
    }
}
