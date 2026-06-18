using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Domain.Entities;

public sealed class Document
{
    public Guid Id { get; init; }
    public required string Title { get; set; }
    public required string Url { get; set; }
    public required DocumentSource Source { get; set; }
    public required string SpaceKey { get; set; }
    public required DocumentType DocumentType { get; set; }
    public required Audience Audience { get; set; }
    public required string System { get; set; }
    public required int Version { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }

    public List<DocumentChunk> Chunks { get; init; } = [];
}
