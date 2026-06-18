using KnowledgeAi.Application.Common.Interfaces;

namespace KnowledgeAi.Api.Contracts;

public sealed record LoginResponse(string AccessToken, string TokenType, int ExpiresIn)
{
    public static LoginResponse FromAccessToken(AccessToken token) =>
        new(token.Value, token.TokenType, token.ExpiresInSeconds);
}
