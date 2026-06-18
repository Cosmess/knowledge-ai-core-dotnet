using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Mcp.Search;

public sealed record SearchApiRequest(
    string Query,
    KnowledgeDomain Domain,
    Audience Audience,
    string? SpaceKey,
    string? System,
    int Limit);
