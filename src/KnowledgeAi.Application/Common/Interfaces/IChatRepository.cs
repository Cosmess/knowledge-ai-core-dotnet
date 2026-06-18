using KnowledgeAi.Domain.Entities;

namespace KnowledgeAi.Application.Common.Interfaces;

public interface IChatRepository
{
    Task<ChatSession> GetOrCreateSessionAsync(Guid? sessionId, Guid userId, CancellationToken cancellationToken);

    Task SaveMessageAsync(ChatMessage message, CancellationToken cancellationToken);
}
