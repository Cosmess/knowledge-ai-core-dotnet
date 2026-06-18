using KnowledgeAi.Domain.Entities;

namespace KnowledgeAi.Application.Common.Interfaces;

public sealed record AccessToken(string Value, string TokenType, int ExpiresInSeconds);

public interface IJwtTokenGenerator
{
    AccessToken GenerateToken(User user);
}
