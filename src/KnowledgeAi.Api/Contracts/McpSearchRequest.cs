using KnowledgeAi.Application.Search.Queries;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Api.Contracts;

public sealed record McpSearchRequest(string Query, KnowledgeDomain? Domain, Audience? Audience, string? SpaceKey, string? System, int? Limit)
{
    public SearchMcpQuery ToQuery() => new(Query, Domain, Audience, SpaceKey, System, Limit ?? 5);
}
