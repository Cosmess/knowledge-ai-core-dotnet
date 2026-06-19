using FluentAssertions;
using KnowledgeAi.Application.Common.Exceptions;
using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Search.Queries;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;
using NSubstitute;

namespace KnowledgeAi.UnitTests.Application.Search.Queries;

public class SearchMcpQueryHandlerTests
{
    private readonly IEmbeddingProvider _embeddingProvider = Substitute.For<IEmbeddingProvider>();
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly ILlmMetricsRecorder _metricsRecorder = Substitute.For<ILlmMetricsRecorder>();
    private readonly SearchMcpQueryHandler _handler;

    public SearchMcpQueryHandlerTests()
    {
        _handler = new SearchMcpQueryHandler(_embeddingProvider, _documentRepository, _currentUser, _metricsRecorder);
        _embeddingProvider.EmbedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new float[1536]);
        _currentUser.AllowedSpaceKeys.Returns(new HashSet<string> { "ENG" });
    }

    [Fact]
    public async Task Handle_WhenTopScoreMeetsThreshold_ReturnsFound()
    {
        _documentRepository
            .SearchAsync(Arg.Any<DocumentSearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] { BuildResult(score: 0.15) });

        var result = await _handler.Handle(new SearchMcpQuery("query", null, null, null, null), CancellationToken.None);

        result.EvidenceStatus.Should().Be(EvidenceStatus.Found);
        _metricsRecorder.Received(1).RecordEvidenceOutcome(hasEnoughEvidence: true);
    }

    [Fact]
    public async Task Handle_WhenTopScoreIsBelowThreshold_ReturnsInsufficient()
    {
        _documentRepository
            .SearchAsync(Arg.Any<DocumentSearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] { BuildResult(score: 0.14) });

        var result = await _handler.Handle(new SearchMcpQuery("query", null, null, null, null), CancellationToken.None);

        result.EvidenceStatus.Should().Be(EvidenceStatus.Insufficient);
    }

    [Fact]
    public async Task Handle_WhenNoResults_ReturnsInsufficient()
    {
        _documentRepository
            .SearchAsync(Arg.Any<DocumentSearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<DocumentChunkSearchResult>());

        var result = await _handler.Handle(new SearchMcpQuery("query", null, null, null, null), CancellationToken.None);

        result.EvidenceStatus.Should().Be(EvidenceStatus.Insufficient);
    }

    [Fact]
    public async Task Handle_WhenDomainIsNotSpecified_DefaultsToTechnical()
    {
        _documentRepository
            .SearchAsync(Arg.Any<DocumentSearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<DocumentChunkSearchResult>());

        var result = await _handler.Handle(new SearchMcpQuery("query", null, null, null, null), CancellationToken.None);

        result.Domain.Should().Be(KnowledgeDomain.Technical);
    }

    [Fact]
    public async Task Handle_WhenSpaceKeyIsNotAllowed_ThrowsForbiddenAccessException()
    {
        var act = () => _handler.Handle(new SearchMcpQuery("query", null, null, "OTHER", null), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenSpaceKeyIsAllowed_PassesAllowedSpaceKeysToSearch()
    {
        _documentRepository
            .SearchAsync(Arg.Any<DocumentSearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<DocumentChunkSearchResult>());

        await _handler.Handle(new SearchMcpQuery("query", null, null, "ENG", null), CancellationToken.None);

        await _documentRepository.Received(1).SearchAsync(
            Arg.Is<DocumentSearchQuery>(q => q.AllowedSpaceKeys!.Contains("ENG")), Arg.Any<CancellationToken>());
    }

    private static DocumentChunkSearchResult BuildResult(double score)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = "Test doc",
            Url = "https://docs.local/test",
            Source = DocumentSource.Markdown,
            SpaceKey = "ENG",
            DocumentType = DocumentType.TechnicalDoc,
            Audience = Audience.Developers,
            System = "knowledge-ai",
            Version = 1,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var chunk = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            Content = "relevant content",
            Embedding = new float[1536],
            Domain = KnowledgeDomain.Technical,
            Metadata = new Dictionary<string, string>(),
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        return new DocumentChunkSearchResult(chunk, document, score);
    }
}
