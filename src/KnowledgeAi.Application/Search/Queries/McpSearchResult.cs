using KnowledgeAi.Application.Common.Models;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Search.Queries;

public sealed record McpSearchResult(
    string Query,
    KnowledgeDomain Domain,
    IReadOnlyList<SourceReference> Results,
    EvidenceStatus EvidenceStatus);
