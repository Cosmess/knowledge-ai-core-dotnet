namespace KnowledgeAi.Application.Common.Interfaces;

public interface ICacheService
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken);

    Task SetAsync(string key, string value, TimeSpan timeToLive, CancellationToken cancellationToken);
}
