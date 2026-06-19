using KnowledgeAi.Application.Common.Exceptions;
using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Application.Common.Models;
using KnowledgeAi.Application.Common.Services;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Chat.Commands;

public sealed class AskQuestionCommandHandler : IRequestHandler<AskQuestionCommand, AskQuestionResult>
{
    // Mirrors the 0.15 evidence threshold from the original rag.service.ts.
    private const double EvidenceThreshold = 0.15;
    private const int MaxResults = 5;

    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILlmProvider _llmProvider;
    private readonly IQuestionClassifier _questionClassifier;
    private readonly IChatRepository _chatRepository;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ILlmMetricsRecorder _metricsRecorder;

    public AskQuestionCommandHandler(
        IEmbeddingProvider embeddingProvider,
        IDocumentRepository documentRepository,
        ILlmProvider llmProvider,
        IQuestionClassifier questionClassifier,
        IChatRepository chatRepository,
        ICurrentUserAccessor currentUser,
        ILlmMetricsRecorder metricsRecorder)
    {
        _embeddingProvider = embeddingProvider;
        _documentRepository = documentRepository;
        _llmProvider = llmProvider;
        _questionClassifier = questionClassifier;
        _chatRepository = chatRepository;
        _currentUser = currentUser;
        _metricsRecorder = metricsRecorder;
    }

    public async Task<AskQuestionResult> Handle(AskQuestionCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.AllowedSpaceKeys.Contains(request.SpaceKey))
        {
            throw new ForbiddenAccessException($"User does not have access to space '{request.SpaceKey}'.");
        }

        var domain = _questionClassifier.Classify(request.Question);
        var queryEmbedding = await _embeddingProvider.EmbedAsync(request.Question, cancellationToken);

        var searchResults = await _documentRepository.SearchAsync(
            new DocumentSearchQuery(
                queryEmbedding, request.Question, domain, request.Audience, request.SpaceKey, request.System, MaxResults, _currentUser.AllowedSpaceKeys),
            cancellationToken);

        var hasEnoughEvidence = searchResults.Count > 0 && searchResults[0].Score >= EvidenceThreshold;
        var evidenceStatus = hasEnoughEvidence ? EvidenceStatus.Found : EvidenceStatus.Insufficient;
        _metricsRecorder.RecordEvidenceOutcome(hasEnoughEvidence);

        var (answer, confidence) = hasEnoughEvidence
            ? await BuildAnswerAsync(request, searchResults, cancellationToken)
            : ("Não há evidência suficiente nas fontes recuperadas para responder com confiança.", 0d);

        var session = await _chatRepository.GetOrCreateSessionAsync(request.ChatSessionId, _currentUser.UserId, cancellationToken);

        await _chatRepository.SaveMessageAsync(new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatSessionId = session.Id,
            Question = request.Question,
            Answer = answer,
            Domain = domain,
            EvidenceStatus = evidenceStatus,
            Confidence = confidence,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        var sources = searchResults
            .Select(result => new SourceReference(result.Document.Title, result.Document.Url, result.Score))
            .ToList();

        return new AskQuestionResult(answer, domain, sources, evidenceStatus, confidence);
    }

    private async Task<(string Answer, double Confidence)> BuildAnswerAsync(
        AskQuestionCommand request,
        IReadOnlyList<DocumentChunkSearchResult> searchResults,
        CancellationToken cancellationToken)
    {
        var context = string.Join("\n\n", searchResults.Select(result => result.Chunk.Content));
        var userPrompt = $"Contexto:\n{context}\n\nPergunta: {request.Question}";

        try
        {
            var completion = await _llmProvider.CompleteAsync(BuildSystemPrompt(request.Audience), userPrompt, cancellationToken);
            _metricsRecorder.RecordCompletion(_llmProvider.ProviderName, completion.InputTokens, completion.OutputTokens, wasFallback: false);
            return (completion.Text, searchResults[0].Score);
        }
        catch
        {
            // Fallback extrativo: devolve o trecho mais relevante quando o provider de LLM falha.
            _metricsRecorder.RecordCompletion(_llmProvider.ProviderName, null, null, wasFallback: true);
            return (searchResults[0].Chunk.Content, searchResults[0].Score);
        }
    }

    private static string BuildSystemPrompt(Audience audience) => audience switch
    {
        Audience.Developers =>
            "Use apenas o contexto recuperado, cite fontes, separe regras de negócio de detalhes técnicos, " +
            "mencione quando faltar evidência, nunca exponha segredos, identifique conflitos entre documentos.",
        _ =>
            "Use apenas o contexto recuperado, explique em linguagem simples, priorize processo e regra de negócio, " +
            "evite detalhes internos de infraestrutura, recomende validação com o time responsável quando houver risco operacional."
    };
}
