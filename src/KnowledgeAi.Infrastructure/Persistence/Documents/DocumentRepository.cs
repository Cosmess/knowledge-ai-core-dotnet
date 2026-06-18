using System.Text.Json;
using Dapper;
using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;
using Npgsql;
using Pgvector;

namespace KnowledgeAi.Infrastructure.Persistence.Documents;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public DocumentRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Document> UpsertDocumentAsync(Document document, CancellationToken cancellationToken)
    {
        const string sql = """
            insert into documents (id, title, url, source, space_key, document_type, audience, system, version, updated_at, created_at)
            values (@Id, @Title, @Url, @Source, @SpaceKey, @DocumentType, @Audience, @System, @Version, @UpdatedAt, @CreatedAt)
            on conflict (url) do update set
                title = excluded.title,
                source = excluded.source,
                space_key = excluded.space_key,
                document_type = excluded.document_type,
                audience = excluded.audience,
                system = excluded.system,
                version = excluded.version,
                updated_at = excluded.updated_at
            returning id, title, url, source, space_key, document_type, audience, system, version, updated_at, created_at;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleAsync<DocumentRow>(new CommandDefinition(sql, new
        {
            document.Id,
            document.Title,
            document.Url,
            Source = document.Source.ToString(),
            document.SpaceKey,
            DocumentType = document.DocumentType.ToString(),
            Audience = document.Audience.ToString(),
            document.System,
            document.Version,
            UpdatedAt = document.UpdatedAt.UtcDateTime,
            CreatedAt = document.CreatedAt.UtcDateTime,
        }, cancellationToken: cancellationToken));

        return row.ToEntity();
    }

    public async Task SaveChunksAsync(Guid documentId, IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken)
    {
        const string deleteSql = "delete from document_chunks where document_id = @documentId;";
        const string insertSql = """
            insert into document_chunks (id, document_id, content, embedding, domain, metadata, created_at, updated_at)
            values (@Id, @DocumentId, @Content, @Embedding, @Domain, @Metadata::jsonb, @CreatedAt, @UpdatedAt);
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(deleteSql, new { documentId }, transaction, cancellationToken: cancellationToken));

        foreach (var chunk in chunks)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                chunk.Id,
                DocumentId = documentId,
                chunk.Content,
                Embedding = new Vector(chunk.Embedding),
                Domain = chunk.Domain.ToString(),
                Metadata = JsonSerializer.Serialize(chunk.Metadata),
                CreatedAt = chunk.CreatedAt.UtcDateTime,
                UpdatedAt = chunk.UpdatedAt.UtcDateTime,
            }, transaction, cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentChunkSearchResult>> SearchAsync(DocumentSearchQuery query, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                dc.id as ChunkId, dc.document_id as DocumentId, dc.content as Content, dc.embedding as Embedding,
                dc.domain as Domain, dc.metadata as Metadata, dc.created_at as ChunkCreatedAt, dc.updated_at as ChunkUpdatedAt,
                d.title as Title, d.url as Url, d.source as Source, d.space_key as SpaceKey,
                d.document_type as DocumentType, d.audience as Audience, d.system as System,
                d.version as Version, d.updated_at as DocUpdatedAt, d.created_at as DocCreatedAt,
                1 - (dc.embedding <=> @QueryEmbedding) as Score
            from document_chunks dc
            join documents d on d.id = dc.document_id
            where (@Domain::text is null or dc.domain = @Domain)
              and (@Audience::text is null or d.audience = @Audience)
              and (@SpaceKey::text is null or d.space_key = @SpaceKey)
              and (@System::text is null or d.system = @System)
            order by dc.embedding <=> @QueryEmbedding
            limit @Limit;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<SearchRow>(new CommandDefinition(sql, new
        {
            QueryEmbedding = new Vector(query.QueryEmbedding),
            Domain = query.Domain?.ToString(),
            Audience = query.Audience?.ToString(),
            query.SpaceKey,
            query.System,
            query.Limit,
        }, cancellationToken: cancellationToken));

        return rows.Select(row => row.ToSearchResult()).ToList();
    }

    public async Task<IReadOnlyList<Document>> ListDocumentsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            select id, title, url, source, space_key, document_type, audience, system, version, updated_at, created_at
            from documents
            order by created_at desc;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<DocumentRow>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.Select(row => row.ToEntity()).ToList();
    }

    public async Task<Document?> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            select id, title, url, source, space_key, document_type, audience, system, version, updated_at, created_at
            from documents
            where id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<DocumentRow>(new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));
        return row?.ToEntity();
    }

    public async Task<Document?> GetByUrlAsync(string url, CancellationToken cancellationToken)
    {
        const string sql = """
            select id, title, url, source, space_key, document_type, audience, system, version, updated_at, created_at
            from documents
            where url = @url;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<DocumentRow>(new CommandDefinition(sql, new { url }, cancellationToken: cancellationToken));
        return row?.ToEntity();
    }

    public async Task<IReadOnlyList<string>> ListSpacesAsync(CancellationToken cancellationToken)
    {
        const string sql = "select distinct space_key from documents order by space_key;";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        var spaces = await connection.QueryAsync<string>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return spaces.ToList();
    }

    private sealed class DocumentRow
    {
        public Guid Id { get; init; }
        public required string Title { get; init; }
        public required string Url { get; init; }
        public required string Source { get; init; }
        public required string SpaceKey { get; init; }
        public required string DocumentType { get; init; }
        public required string Audience { get; init; }
        public required string System { get; init; }
        public int Version { get; init; }
        public DateTime UpdatedAt { get; init; }
        public DateTime CreatedAt { get; init; }

        public Document ToEntity() => new()
        {
            Id = Id,
            Title = Title,
            Url = Url,
            Source = Enum.Parse<DocumentSource>(Source, ignoreCase: true),
            SpaceKey = SpaceKey,
            DocumentType = Enum.Parse<DocumentType>(DocumentType, ignoreCase: true),
            Audience = Enum.Parse<Audience>(Audience, ignoreCase: true),
            System = System,
            Version = Version,
            UpdatedAt = new DateTimeOffset(UpdatedAt, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(CreatedAt, TimeSpan.Zero),
        };
    }

    private sealed class SearchRow
    {
        public Guid ChunkId { get; init; }
        public Guid DocumentId { get; init; }
        public required string Content { get; init; }
        public required Vector Embedding { get; init; }
        public required string Domain { get; init; }
        public required string Metadata { get; init; }
        public DateTime ChunkCreatedAt { get; init; }
        public DateTime ChunkUpdatedAt { get; init; }
        public required string Title { get; init; }
        public required string Url { get; init; }
        public required string Source { get; init; }
        public required string SpaceKey { get; init; }
        public required string DocumentType { get; init; }
        public required string Audience { get; init; }
        public required string System { get; init; }
        public int Version { get; init; }
        public DateTime DocUpdatedAt { get; init; }
        public DateTime DocCreatedAt { get; init; }
        public double Score { get; init; }

        public DocumentChunkSearchResult ToSearchResult()
        {
            var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(Metadata) ?? new Dictionary<string, string>();

            var chunk = new DocumentChunk
            {
                Id = ChunkId,
                DocumentId = DocumentId,
                Content = Content,
                Embedding = Embedding.ToArray(),
                Domain = Enum.Parse<KnowledgeDomain>(Domain, ignoreCase: true),
                Metadata = metadata,
                CreatedAt = new DateTimeOffset(ChunkCreatedAt, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(ChunkUpdatedAt, TimeSpan.Zero),
            };

            var document = new Document
            {
                Id = DocumentId,
                Title = Title,
                Url = Url,
                Source = Enum.Parse<DocumentSource>(Source, ignoreCase: true),
                SpaceKey = SpaceKey,
                DocumentType = Enum.Parse<DocumentType>(DocumentType, ignoreCase: true),
                Audience = Enum.Parse<Audience>(Audience, ignoreCase: true),
                System = System,
                Version = Version,
                UpdatedAt = new DateTimeOffset(DocUpdatedAt, TimeSpan.Zero),
                CreatedAt = new DateTimeOffset(DocCreatedAt, TimeSpan.Zero),
            };

            return new DocumentChunkSearchResult(chunk, document, Score);
        }
    }
}
