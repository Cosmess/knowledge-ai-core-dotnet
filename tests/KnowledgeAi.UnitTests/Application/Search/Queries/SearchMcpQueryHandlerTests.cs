using FluentAssertions;
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
    private readonly SearchMcpQueryHandler _handler;

    public SearchMcpQueryHandlerTests()
    {
        _handler = new SearchMcpQueryHandler(_embeddingProvider, _documentRepository);
        _embeddingProvider.EmbedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new float[1536]);
    }

    [Fact]
    public async Task Handle_WhenTopScoreMeetsThreshold_ReturnsFound()
    {
        _documentRepository
            .SearchAsync(Arg.Any<DocumentSearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] { BuildResult(score: 0.15) });

        var result = await _handler.Handle(new SearchMcpQuery("query", null, null, null, null), CancellationToken.None);

        result.EvidenceStatus.Should().Be(EvidenceStatus.Found);
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
