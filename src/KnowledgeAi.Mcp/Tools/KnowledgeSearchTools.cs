using System.ComponentModel;
using System.Text.Json;
using KnowledgeAi.Application.Search.Queries;
using KnowledgeAi.Domain.ValueObjects;
using KnowledgeAi.Mcp.Search;
using ModelContextProtocol.Server;

namespace KnowledgeAi.Mcp.Tools;

[McpServerToolType]
public sealed class KnowledgeSearchTools
{
    private static readonly JsonSerializerOptions ResponseJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly KnowledgeApiClient _client;

    public KnowledgeSearchTools(KnowledgeApiClient client)
    {
        _client = client;
    }

    [McpServerTool(Name = "search_technical_docs")]
    [Description("Search internal knowledge using search_technical_docs.")]
    public Task<string> SearchTechnicalDocs(
        string query,
        string? system = null,
        string? spaceKey = null,
        int limit = 5,
        CancellationToken cancellationToken = default) =>
        SearchAsync(query, KnowledgeDomain.Technical, system, spaceKey, limit, cancellationToken);

    [McpServerTool(Name = "search_business_rules")]
    [Description("Search internal knowledge using search_business_rules.")]
    public Task<string> SearchBusinessRules(
        string query,
        string? system = null,
        string? spaceKey = null,
        int limit = 5,
        CancellationToken cancellationToken = default) =>
        SearchAsync(query, KnowledgeDomain.Business, system, spaceKey, limit, cancellationToken);

    [McpServerTool(Name = "search_api_docs")]
    [Description("Search internal knowledge using search_api_docs.")]
    public Task<string> SearchApiDocs(
        string query,
        string? system = null,
        string? spaceKey = null,
        int limit = 5,
        CancellationToken cancellationToken = default) =>
        SearchAsync(query, KnowledgeDomain.Api, system, spaceKey, limit, cancellationToken);

    [McpServerTool(Name = "search_architecture_docs")]
    [Description("Search internal knowledge using search_architecture_docs.")]
    public Task<string> SearchArchitectureDocs(
        string query,
        string? system = null,
        string? spaceKey = null,
        int limit = 5,
        CancellationToken cancellationToken = default) =>
        SearchAsync(query, KnowledgeDomain.Architecture, system, spaceKey, limit, cancellationToken);

    [McpServerTool(Name = "search_user_stories")]
    [Description("Search internal knowledge using search_user_stories.")]
    public Task<string> SearchUserStories(
        string query,
        string? system = null,
        string? spaceKey = null,
        int limit = 5,
        CancellationToken cancellationToken = default) =>
        SearchAsync(query, KnowledgeDomain.Backlog, system, spaceKey, limit, cancellationToken);

    [McpServerTool(Name = "get_service_context")]
    [Description("Search internal knowledge using get_service_context.")]
    public Task<string> GetServiceContext(
        string query,
        string? system = null,
        string? spaceKey = null,
        int limit = 5,
        CancellationToken cancellationToken = default) =>
        SearchAsync(query, KnowledgeDomain.Technical, system, spaceKey, limit, cancellationToken);

    private async Task<string> SearchAsync(
        string query,
        KnowledgeDomain domain,
        string? system,
        string? spaceKey,
        int limit,
        CancellationToken cancellationToken)
    {
        var result = await _client.SearchAsync(query, domain, Audience.Developers, system, spaceKey, limit, cancellationToken);
        return JsonSerializer.Serialize(result, ResponseJsonOptions);
    }
}
