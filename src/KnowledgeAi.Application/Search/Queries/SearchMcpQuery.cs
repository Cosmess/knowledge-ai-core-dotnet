using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Search.Queries;

public sealed record SearchMcpQuery(
    string Query,
    KnowledgeDomain? Domain,
    Audience? Audience,
    string? SpaceKey,
    string? System,
    int Limit = 5) : IRequest<McpSearchResult>;
