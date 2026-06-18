using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Domain.Entities;

public sealed class User
{
    public Guid Id { get; init; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required Role Role { get; set; }
    public required IReadOnlySet<string> AllowedSpaceKeys { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}
