using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Ingestion.Commands;

public sealed class IngestConfluenceCommandHandler : IRequestHandler<IngestConfluenceCommand, IngestionResult>
{
    private readonly IConfluenceClient _confluenceClient;
    private readonly IContentSanitizer _contentSanitizer;
    private readonly IHtmlNormalizer _htmlNormalizer;
    private readonly IChunkingService _chunkingService;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IDocumentRepository _documentRepository;
    private readonly IIngestionJobRepository _jobRepository;

    public IngestConfluenceCommandHandler(
        IConfluenceClient confluenceClient,
        IContentSanitizer contentSanitizer,
        IHtmlNormalizer htmlNormalizer,
        IChunkingService chunkingService,
        IEmbeddingProvider embeddingProvider,
        IDocumentRepository documentRepository,
        IIngestionJobRepository jobRepository)
    {
        _confluenceClient = confluenceClient;
        _contentSanitizer = contentSanitizer;
        _htmlNormalizer = htmlNormalizer;
        _chunkingService = chunkingService;
        _embeddingProvider = embeddingProvider;
        _documentRepository = documentRepository;
        _jobRepository = jobRepository;
    }

    public async Task<IngestionResult> Handle(IngestConfluenceCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.CreateAsync(new IngestionJob
        {
            Id = Guid.NewGuid(),
            Source = DocumentSource.Confluence,
            SpaceKey = request.SpaceKey,
            Status = IngestionJobStatus.Running,
            StartedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        var pages = await _confluenceClient.FetchPagesAsync(request.SpaceKey, cancellationToken);

        var documentsProcessed = 0;
        var chunksProcessed = 0;

        foreach (var page in pages)
        {
            // Reindexação incremental: só reprocessa a página se a versão do Confluence avançou
            // (corrige a divergência do projeto original, onde toda ingestão era sempre completa).
            var existingDocument = await _documentRepository.GetByUrlAsync(page.Url, cancellationToken);
            if (existingDocument is not null && existingDocument.Version >= page.Version)
            {
                continue;
            }

            var document = await _documentRepository.UpsertDocumentAsync(
                BuildDocument(page, request.SpaceKey, existingDocument), cancellationToken);

            var chunkCount = await ChunkAndSaveAsync(document, page, cancellationToken);

            documentsProcessed++;
            chunksProcessed += chunkCount;
        }

        job.Status = IngestionJobStatus.Completed;
        job.DocumentsProcessed = documentsProcessed;
        job.ChunksProcessed = chunksProcessed;
        job.CompletedAt = DateTimeOffset.UtcNow;
        await _jobRepository.UpdateAsync(job, cancellationToken);

        return new IngestionResult(job.Id, documentsProcessed, chunksProcessed);
    }

    private async Task<int> ChunkAndSaveAsync(Document document, ConfluencePage page, CancellationToken cancellationToken)
    {
        var sanitizedHtml = _contentSanitizer.Sanitize(page.BodyHtml);
        var plainText = _htmlNormalizer.ToPlainText(sanitizedHtml);
        var chunks = _chunkingService.Split(plainText);
        var documentChunks = new List<DocumentChunk>(chunks.Count);

        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingProvider.EmbedAsync(chunk.Content, cancellationToken);

            documentChunks.Add(new DocumentChunk
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Content = chunk.Content,
                Embedding = embedding,
                Domain = KnowledgeDomain.Business,
                Metadata = chunk.HeadingPath is null
                    ? new Dictionary<string, string>()
                    : new Dictionary<string, string> { ["headingPath"] = chunk.HeadingPath },
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        await _documentRepository.SaveChunksAsync(document.Id, documentChunks, cancellationToken);
        return documentChunks.Count;
    }

    private static Document BuildDocument(ConfluencePage page, string spaceKey, Document? existingDocument) => new()
    {
        Id = existingDocument?.Id ?? Guid.NewGuid(),
        Title = page.Title,
        Url = page.Url,
        Source = DocumentSource.Confluence,
        SpaceKey = spaceKey,
        DocumentType = existingDocument?.DocumentType ?? DocumentType.BusinessRule,
        Audience = existingDocument?.Audience ?? Audience.Operations,
        System = existingDocument?.System ?? spaceKey,
        Version = page.Version,
        UpdatedAt = page.UpdatedAt,
        CreatedAt = existingDocument?.CreatedAt ?? DateTimeOffset.UtcNow
    };
}
