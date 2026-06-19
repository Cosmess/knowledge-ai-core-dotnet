using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Common.Interfaces;

public sealed record DocumentSearchQuery(
    float[] QueryEmbedding,
    string QueryText,
    KnowledgeDomain? Domain,
    Audience? Audience,
    string? SpaceKey,
    string? System,
    int Limit,
    IReadOnlyCollection<string>? AllowedSpaceKeys = null);

public sealed record DocumentChunkSearchResult(DocumentChunk Chunk, Document Document, double Score);

public interface IDocumentRepository
{
    Task<Document> UpsertDocumentAsync(Document document, CancellationToken cancellationToken);

    Task SaveChunksAsync(Guid documentId, IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentChunkSearchResult>> SearchAsync(DocumentSearchQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<Document>> ListDocumentsAsync(CancellationToken cancellationToken);

    Task<Document?> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Document?> GetByUrlAsync(string url, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListSpacesAsync(CancellationToken cancellationToken);
}
