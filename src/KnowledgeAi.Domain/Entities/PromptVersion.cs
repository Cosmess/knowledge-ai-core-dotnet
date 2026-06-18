using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Domain.Entities;

public sealed class PromptVersion
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required Audience Audience { get; set; }
    public required KnowledgeDomain Domain { get; set; }
    public required string Template { get; set; }
    public required int Version { get; set; }
    public required bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}
