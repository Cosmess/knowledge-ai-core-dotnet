using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Api.Contracts;

public sealed record UserResponse(Guid Id, string Email, Role Role, IReadOnlySet<string> AllowedSpaceKeys, DateTimeOffset CreatedAt)
{
    public static UserResponse FromUser(User user) =>
        new(user.Id, user.Email, user.Role, user.AllowedSpaceKeys, user.CreatedAt);
}
