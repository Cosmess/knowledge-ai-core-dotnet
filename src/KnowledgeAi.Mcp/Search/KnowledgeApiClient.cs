using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using KnowledgeAi.Application.Search.Queries;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Mcp.Search;

public sealed class KnowledgeApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private readonly HttpClient _httpClient;

    public KnowledgeApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<McpSearchResult> SearchAsync(
        string query,
        KnowledgeDomain domain,
        Audience audience,
        string? system,
        string? spaceKey,
        int limit,
        CancellationToken cancellationToken)
    {
        var request = new SearchApiRequest(query, domain, audience, spaceKey, system, limit);

        using var response = await _httpClient.PostAsJsonAsync("mcp/search", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<McpSearchResult>(JsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Knowledge API returned an empty search result.");
    }
}
