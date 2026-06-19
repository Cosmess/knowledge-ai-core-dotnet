using KnowledgeAi.Application.Common.Exceptions;
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
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ILlmMetricsRecorder _metricsRecorder;

    public SearchMcpQueryHandler(
        IEmbeddingProvider embeddingProvider,
        IDocumentRepository documentRepository,
        ICurrentUserAccessor currentUser,
        ILlmMetricsRecorder metricsRecorder)
    {
        _embeddingProvider = embeddingProvider;
        _documentRepository = documentRepository;
        _currentUser = currentUser;
        _metricsRecorder = metricsRecorder;
    }

    public async Task<McpSearchResult> Handle(SearchMcpQuery request, CancellationToken cancellationToken)
    {
        if (request.SpaceKey is not null && !_currentUser.AllowedSpaceKeys.Contains(request.SpaceKey))
        {
            throw new ForbiddenAccessException($"User does not have access to space '{request.SpaceKey}'.");
        }

        var domain = request.Domain ?? KnowledgeDomain.Technical;
        var queryEmbedding = await _embeddingProvider.EmbedAsync(request.Query, cancellationToken);

        var searchResults = await _documentRepository.SearchAsync(
            new DocumentSearchQuery(
                queryEmbedding, request.Query, domain, request.Audience, request.SpaceKey, request.System, request.Limit, _currentUser.AllowedSpaceKeys),
            cancellationToken);

        var hasEnoughEvidence = searchResults.Count > 0 && searchResults[0].Score >= EvidenceThreshold;
        _metricsRecorder.RecordEvidenceOutcome(hasEnoughEvidence);
        var evidenceStatus = hasEnoughEvidence ? EvidenceStatus.Found : EvidenceStatus.Insufficient;

        var sources = searchResults
            .Select(result => new SourceReference(result.Document.Title, result.Document.Url, result.Score))
            .ToList();

        return new McpSearchResult(request.Query, domain, sources, evidenceStatus);
    }
}
