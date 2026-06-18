namespace KnowledgeAi.Infrastructure.Ingestion;

public sealed class ConfluenceOptions
{
    public const string SectionName = "Confluence";

    public string BaseUrl { get; set; } = "https://example.atlassian.net/wiki/";
    public string? Email { get; set; }
    public string? ApiToken { get; set; }
}
