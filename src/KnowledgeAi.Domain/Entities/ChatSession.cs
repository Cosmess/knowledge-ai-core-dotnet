namespace KnowledgeAi.Domain.Entities;

public sealed class ChatSession
{
    public Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public List<ChatMessage> Messages { get; init; } = [];
}
