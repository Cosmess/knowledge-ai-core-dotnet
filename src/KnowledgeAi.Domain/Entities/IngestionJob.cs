using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Domain.Entities;

public enum IngestionJobStatus
{
    Pending,
    Running,
    Completed,
    Failed
}

public sealed class IngestionJob
{
    public Guid Id { get; init; }
    public required DocumentSource Source { get; init; }
    public required string SpaceKey { get; init; }
    public IngestionJobStatus Status { get; set; } = IngestionJobStatus.Pending;
    public int DocumentsProcessed { get; set; }
    public int ChunksProcessed { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; set; }
}
