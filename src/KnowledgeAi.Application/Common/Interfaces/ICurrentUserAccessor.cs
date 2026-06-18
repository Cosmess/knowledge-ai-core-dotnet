using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Common.Interfaces;

public interface ICurrentUserAccessor
{
    Guid UserId { get; }

    Role Role { get; }

    IReadOnlySet<string> AllowedSpaceKeys { get; }
}
