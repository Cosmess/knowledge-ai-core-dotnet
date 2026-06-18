namespace KnowledgeAi.Domain.Entities;

public sealed class Feedback
{
    public Guid Id { get; init; }
    public required Guid ChatMessageId { get; init; }
    public required Guid UserId { get; init; }
    public required bool Helpful { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}
