namespace KnowledgeAi.Api.Auth;

public sealed class ApiKeyOptions
{
    public const string SectionName = "ApiKey";

    public required string Value { get; set; }
}
