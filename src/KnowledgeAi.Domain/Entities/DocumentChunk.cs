using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Domain.Entities;

public sealed class DocumentChunk
{
    public Guid Id { get; init; }
    public required Guid DocumentId { get; init; }
    public required string Content { get; set; }
    public required float[] Embedding { get; set; }
    public required KnowledgeDomain Domain { get; set; }
    public required IReadOnlyDictionary<string, string> Metadata { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
