namespace KnowledgeAi.Application.Common.Interfaces;

public sealed record ConfluencePage(string Title, string BodyHtml, int Version, string Url, DateTimeOffset UpdatedAt);

public interface IConfluenceClient
{
    Task<IReadOnlyList<ConfluencePage>> FetchPagesAsync(string spaceKey, CancellationToken cancellationToken);
}
