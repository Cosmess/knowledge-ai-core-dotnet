namespace KnowledgeAi.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string SigningKey { get; set; }
    public string Issuer { get; set; } = "knowledge-ai-core";
    public string Audience { get; set; } = "knowledge-ai-clients";
    public int ExpiresInSeconds { get; set; } = 3600;
}
