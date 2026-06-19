using FluentAssertions;
using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Ingestion.Commands;
using KnowledgeAi.Domain.Entities;
using NSubstitute;

namespace KnowledgeAi.UnitTests.Application.Ingestion.Commands;

public class IngestConfluenceCommandHandlerTests
{
    private readonly IConfluenceClient _confluenceClient = Substitute.For<IConfluenceClient>();
    private readonly IContentSanitizer _contentSanitizer = Substitute.For<IContentSanitizer>();
    private readonly IHtmlNormalizer _htmlNormalizer = Substitute.For<IHtmlNormalizer>();
    private readonly IChunkingService _chunkingService = Substitute.For<IChunkingService>();
    private readonly IEmbeddingProvider _embeddingProvider = Substitute.For<IEmbeddingProvider>();
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly IIngestionJobRepository _jobRepository = Substitute.For<IIngestionJobRepository>();
    private readonly IngestConfluenceCommandHandler _handler;

    public IngestConfluenceCommandHandlerTests()
    {
        _handler = new IngestConfluenceCommandHandler(
            _confluenceClient, _contentSanitizer, _htmlNormalizer, _chunkingService, _embeddingProvider, _documentRepository, _jobRepository);

        _jobRepository.CreateAsync(Arg.Any<IngestionJob>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<IngestionJob>()));
        _documentRepository.GetByUrlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Document?)null);
        _documentRepository.UpsertDocumentAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Document>()));
        _embeddingProvider.EmbedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new float[1536]);
        _chunkingService.Split(Arg.Any<string>()).Returns(new[] { new TextChunk("chunk content", "Heading") });
    }

    [Fact]
    public async Task Handle_SanitizesPageHtmlBeforeNormalizing()
    {
        var page = new ConfluencePage("Title", "<script>alert(1)</script><h1>Title</h1>", 1, "https://confluence.local/page", DateTimeOffset.UtcNow);
        _confluenceClient.FetchPagesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new[] { page });
        _contentSanitizer.Sanitize(page.BodyHtml).Returns("<h1>Title</h1>");

        await _handler.Handle(new IngestConfluenceCommand("ENG"), CancellationToken.None);

        _contentSanitizer.Received(1).Sanitize(page.BodyHtml);
        _htmlNormalizer.Received(1).ToPlainText("<h1>Title</h1>");
    }

    [Fact]
    public async Task Handle_SavesOneDocumentAndItsChunks()
    {
        var page = new ConfluencePage("Title", "<h1>Title</h1><p>Body</p>", 1, "https://confluence.local/page", DateTimeOffset.UtcNow);
        _confluenceClient.FetchPagesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new[] { page });
        _contentSanitizer.Sanitize(Arg.Any<string>()).Returns(page.BodyHtml);
        _htmlNormalizer.ToPlainText(Arg.Any<string>()).Returns("Title\nBody");

        var result = await _handler.Handle(new IngestConfluenceCommand("ENG"), CancellationToken.None);

        result.DocumentsProcessed.Should().Be(1);
        result.ChunksProcessed.Should().Be(1);
        await _documentRepository.Received(1).SaveChunksAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<DocumentChunk>>(), Arg.Any<CancellationToken>());
    }
}
