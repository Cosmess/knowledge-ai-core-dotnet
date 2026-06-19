using KnowledgeAi.Application.Common;
using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Ingestion.Commands;

public sealed class IngestMarkdownCommandHandler : IRequestHandler<IngestMarkdownCommand, IngestionResult>
{
    private readonly IMarkdownLoader _markdownLoader;
    private readonly IContentSanitizer _contentSanitizer;
    private readonly IChunkingService _chunkingService;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IDocumentRepository _documentRepository;
    private readonly IIngestionJobRepository _jobRepository;

    public IngestMarkdownCommandHandler(
        IMarkdownLoader markdownLoader,
        IContentSanitizer contentSanitizer,
        IChunkingService chunkingService,
        IEmbeddingProvider embeddingProvider,
        IDocumentRepository documentRepository,
        IIngestionJobRepository jobRepository)
    {
        _markdownLoader = markdownLoader;
        _contentSanitizer = contentSanitizer;
        _chunkingService = chunkingService;
        _embeddingProvider = embeddingProvider;
        _documentRepository = documentRepository;
        _jobRepository = jobRepository;
    }

    public async Task<IngestionResult> Handle(IngestMarkdownCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.CreateAsync(new IngestionJob
        {
            Id = Guid.NewGuid(),
            Source = DocumentSource.Markdown,
            SpaceKey = request.SpaceKey,
            Status = IngestionJobStatus.Running,
            StartedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        var markdownDocuments = await _markdownLoader.LoadAsync(request.RootDir, cancellationToken);

        var documentsProcessed = 0;
        var chunksProcessed = 0;

        foreach (var markdownDocument in markdownDocuments)
        {
            var document = await _documentRepository.UpsertDocumentAsync(
                BuildDocument(markdownDocument, request.SpaceKey),
                cancellationToken);

            var chunkCount = await ChunkAndSaveAsync(document, markdownDocument, cancellationToken);

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

    private async Task<int> ChunkAndSaveAsync(Document document, MarkdownDocument markdownDocument, CancellationToken cancellationToken)
    {
        var domain = MetadataParsing.ParseEnumOrDefault(
            markdownDocument.Frontmatter.GetValueOrDefault("domain"), KnowledgeDomain.Technical);

        var sanitizedContent = _contentSanitizer.Sanitize(markdownDocument.Content);
        var chunks = _chunkingService.Split(sanitizedContent);
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
                Domain = domain,
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

    private static Document BuildDocument(MarkdownDocument markdownDocument, string spaceKey)
    {
        var frontmatter = markdownDocument.Frontmatter;

        return new Document
        {
            Id = Guid.NewGuid(),
            Title = markdownDocument.Title,
            Url = markdownDocument.FilePath,
            Source = DocumentSource.Markdown,
            SpaceKey = spaceKey,
            DocumentType = MetadataParsing.ParseEnumOrDefault(
                frontmatter.GetValueOrDefault("documentType"), DocumentType.TechnicalDoc),
            Audience = MetadataParsing.ParseEnumOrDefault(
                frontmatter.GetValueOrDefault("audience"), Audience.Developers),
            System = frontmatter.GetValueOrDefault("system", "unknown"),
            Version = 1,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
