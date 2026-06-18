using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Application.Common.Models;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Search.Queries;

public sealed class SearchMcpQueryHandler : IRequestHandler<SearchMcpQuery, McpSearchResult>
{
    // Mirrors the 0.15 evidence threshold used by /chat, kept consistent across both entry points.
    private const double EvidenceThreshold = 0.15;

    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IDocumentRepository _documentRepository;

    public SearchMcpQueryHandler(IEmbeddingProvider embeddingProvider, IDocumentRepository documentRepository)
    {
        _embeddingProvider = embeddingProvider;
        _documentRepository = documentRepository;
    }

    public async Task<McpSearchResult> Handle(SearchMcpQuery request, CancellationToken cancellationToken)
    {
        var domain = request.Domain ?? KnowledgeDomain.Technical;
        var queryEmbedding = await _embeddingProvider.EmbedAsync(request.Query, cancellationToken);

        var searchResults = await _documentRepository.SearchAsync(
            new DocumentSearchQuery(queryEmbedding, domain, request.Audience, request.SpaceKey, request.System, request.Limit),
            cancellationToken);

        var evidenceStatus = searchResults.Count > 0 && searchResults[0].Score >= EvidenceThreshold
            ? EvidenceStatus.Found
            : EvidenceStatus.Insufficient;

        var sources = searchResults
            .Select(result => new SourceReference(result.Document.Title, result.Document.Url, result.Score))
            .ToList();

        return new McpSearchResult(request.Query, domain, sources, evidenceStatus);
    }
}
