using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Domain.Entities;

public sealed class ChatMessage
{
    public Guid Id { get; init; }
    public required Guid ChatSessionId { get; init; }
    public required string Question { get; set; }
    public required string Answer { get; set; }
    public required KnowledgeDomain Domain { get; set; }
    public required EvidenceStatus EvidenceStatus { get; set; }
    public required double Confidence { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}
