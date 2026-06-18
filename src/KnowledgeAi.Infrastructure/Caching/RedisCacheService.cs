using KnowledgeAi.Application.Common.Interfaces;
using StackExchange.Redis;

namespace KnowledgeAi.Infrastructure.Caching;

public sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var value = await database.StringGetAsync(key);
        return value.IsNullOrEmpty ? null : value.ToString();
    }

    public async Task SetAsync(string key, string value, TimeSpan timeToLive, CancellationToken cancellationToken)
    {
        var database = _connectionMultiplexer.GetDatabase();
        await database.StringSetAsync(key, value, timeToLive);
    }
}
