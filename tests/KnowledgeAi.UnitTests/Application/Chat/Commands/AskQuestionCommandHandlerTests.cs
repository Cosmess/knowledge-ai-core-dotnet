using FluentAssertions;
using KnowledgeAi.Application.Chat.Commands;
using KnowledgeAi.Application.Common.Exceptions;
using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Common.Services;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;
using NSubstitute;

namespace KnowledgeAi.UnitTests.Application.Chat.Commands;

public class AskQuestionCommandHandlerTests
{
    private readonly IEmbeddingProvider _embeddingProvider = Substitute.For<IEmbeddingProvider>();
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly ILlmProvider _llmProvider = Substitute.For<ILlmProvider>();
    private readonly IQuestionClassifier _questionClassifier = Substitute.For<IQuestionClassifier>();
    private readonly IChatRepository _chatRepository = Substitute.For<IChatRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly ILlmMetricsRecorder _metricsRecorder = Substitute.For<ILlmMetricsRecorder>();
    private readonly AskQuestionCommandHandler _handler;

    public AskQuestionCommandHandlerTests()
    {
        _handler = new AskQuestionCommandHandler(
            _embeddingProvider, _documentRepository, _llmProvider, _questionClassifier, _chatRepository, _currentUser, _metricsRecorder);

        _embeddingProvider.EmbedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new float[1536]);
        _questionClassifier.Classify(Arg.Any<string>()).Returns(KnowledgeDomain.Technical);
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.AllowedSpaceKeys.Returns(new HashSet<string> { "ENG" });
        _chatRepository
            .GetOrCreateSessionAsync(Arg.Any<Guid?>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new ChatSession { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow });
    }

    [Fact]
    public async Task Handle_WhenTopScoreMeetsThreshold_CallsLlmAndReturnsFound()
    {
        _documentRepository
            .SearchAsync(Arg.Any<DocumentSearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] { BuildResult(score: 0.15) });
        _llmProvider.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new LlmCompletionResult("the answer", InputTokens: 100, OutputTokens: 20));

        var result = await _handler.Handle(
            new AskQuestionCommand("How does deploy work?", Audience.Developers, "ENG", "knowledge-ai"),
            CancellationToken.None);

        result.EvidenceStatus.Should().Be(EvidenceStatus.Found);
        result.Answer.Should().Be("the answer");
        await _llmProvider.Received(1).CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _metricsRecorder.Received(1).RecordCompletion(_llmProvider.ProviderName, 100, 20, wasFallback: false);
    }

    [Fact]
    public async Task Handle_WhenTopScoreIsBelowThreshold_SkipsLlmAndReturnsInsufficient()
    {
        _documentRepository
            .SearchAsync(Arg.Any<DocumentSearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] { BuildResult(score: 0.1) });

        var result = await _handler.Handle(
            new AskQuestionCommand("How does deploy work?", Audience.Developers, "ENG", "knowledge-ai"),
            CancellationToken.None);

        result.EvidenceStatus.Should().Be(EvidenceStatus.Insufficient);
        result.Confidence.Should().Be(0);
        await _llmProvider.DidNotReceive().CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLlmProviderThrows_FallsBackToTopChunkContent()
    {
        _documentRepository
            .SearchAsync(Arg.Any<DocumentSearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] { BuildResult(score: 0.9, content: "extractive fallback content") });
        _llmProvider
            .CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<LlmCompletionResult>(new InvalidOperationException("llm down")));

        var result = await _handler.Handle(
            new AskQuestionCommand("How does deploy work?", Audience.Developers, "ENG", "knowledge-ai"),
            CancellationToken.None);

        result.EvidenceStatus.Should().Be(EvidenceStatus.Found);
        result.Answer.Should().Be("extractive fallback content");
        _metricsRecorder.Received(1).RecordCompletion(_llmProvider.ProviderName, null, null, wasFallback: true);
    }

    [Fact]
    public async Task Handle_WhenSpaceKeyNotAllowed_ThrowsForbiddenAccessException()
    {
        var act = () => _handler.Handle(
            new AskQuestionCommand("How does deploy work?", Audience.Developers, "OTHER", "knowledge-ai"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
        await _documentRepository.DidNotReceive().SearchAsync(Arg.Any<DocumentSearchQuery>(), Arg.Any<CancellationToken>());
    }

    private static DocumentChunkSearchResult BuildResult(double score, string content = "relevant content")
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
            Content = content,
            Embedding = new float[1536],
            Domain = KnowledgeDomain.Technical,
            Metadata = new Dictionary<string, string>(),
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        return new DocumentChunkSearchResult(chunk, document, score);
    }
}
