using System.Net.Http.Json;
using System.Text.Json.Serialization;
using KnowledgeAi.Application.Common.Interfaces;

namespace KnowledgeAi.Infrastructure.Ingestion;

/// <summary>
/// Typed HttpClient for the Confluence REST API. Retry/backoff policy is attached at DI
/// registration time (Polly), and base address/auth headers are configured there too.
/// </summary>
public sealed class ConfluenceHttpClient : IConfluenceClient
{
    private readonly HttpClient _httpClient;

    public ConfluenceHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ConfluencePage>> FetchPagesAsync(string spaceKey, CancellationToken cancellationToken)
    {
        var requestUri = $"rest/api/content?spaceKey={Uri.EscapeDataString(spaceKey)}&expand=body.storage,version,history.lastUpdated&limit=100";
        var response = await _httpClient.GetFromJsonAsync<ConfluenceContentResponse>(requestUri, cancellationToken);

        if (response?.Results is null)
        {
            return [];
        }

        return response.Results.Select(item => new ConfluencePage(
            item.Title ?? "Untitled",
            item.Body?.Storage?.Value ?? string.Empty,
            item.Version?.Number ?? 1,
            BuildUrl(item),
            item.History?.LastUpdated?.When ?? DateTimeOffset.UtcNow)).ToList();
    }

    private string BuildUrl(ConfluenceContentItem item)
    {
        var webUiPath = item.Links?.WebUi;
        return webUiPath is null || _httpClient.BaseAddress is null
            ? webUiPath ?? string.Empty
            : new Uri(_httpClient.BaseAddress, webUiPath).ToString();
    }

    private sealed class ConfluenceContentResponse
    {
        [JsonPropertyName("results")]
        public List<ConfluenceContentItem>? Results { get; set; }
    }

    private sealed class ConfluenceContentItem
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("body")]
        public ConfluenceBody? Body { get; set; }

        [JsonPropertyName("version")]
        public ConfluenceVersion? Version { get; set; }

        [JsonPropertyName("history")]
        public ConfluenceHistory? History { get; set; }

        [JsonPropertyName("_links")]
        public ConfluenceLinks? Links { get; set; }
    }

    private sealed class ConfluenceBody
    {
        [JsonPropertyName("storage")]
        public ConfluenceStorage? Storage { get; set; }
    }

    private sealed class ConfluenceStorage
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    private sealed class ConfluenceVersion
    {
        [JsonPropertyName("number")]
        public int? Number { get; set; }
    }

    private sealed class ConfluenceHistory
    {
        [JsonPropertyName("lastUpdated")]
        public ConfluenceLastUpdated? LastUpdated { get; set; }
    }

    private sealed class ConfluenceLastUpdated
    {
        [JsonPropertyName("when")]
        public DateTimeOffset? When { get; set; }
    }

    private sealed class ConfluenceLinks
    {
        [JsonPropertyName("webui")]
        public string? WebUi { get; set; }
    }
}
