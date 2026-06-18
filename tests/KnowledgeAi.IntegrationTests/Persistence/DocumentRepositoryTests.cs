using FluentAssertions;
using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;
using KnowledgeAi.Infrastructure.Persistence;
using KnowledgeAi.Infrastructure.Persistence.Documents;
using Npgsql;
using Testcontainers.PostgreSql;

namespace KnowledgeAi.IntegrationTests.Persistence;

public sealed class DocumentRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg16")
        .WithDatabase("knowledgeai")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private NpgsqlDataSource _dataSource = null!;
    private DocumentRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _dataSource = NpgsqlDataSourceFactory.Create(_container.GetConnectionString());
        await new DatabaseInitializer(_dataSource).InitializeAsync();

        _repository = new DocumentRepository(_dataSource);
    }

    public async Task DisposeAsync()
    {
        await _dataSource.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task UpsertDocumentAsync_WhenUrlAlreadyExists_KeepsOriginalId()
    {
        var document = BuildDocument(url: "https://docs.local/page-1", version: 1);

        var inserted = await _repository.UpsertDocumentAsync(document, CancellationToken.None);

        var updated = BuildDocument(url: "https://docs.local/page-1", version: 2);
        var upserted = await _repository.UpsertDocumentAsync(updated, CancellationToken.None);

        upserted.Id.Should().Be(inserted.Id);
        upserted.Version.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_ReturnsClosestChunkFirst_ByCosineSimilarity()
    {
        var document = await _repository.UpsertDocumentAsync(
            BuildDocument(url: "https://docs.local/page-2", version: 1), CancellationToken.None);

        var closeEmbedding = BuildEmbedding(seed: 1f);
        var farEmbedding = BuildEmbedding(seed: -1f);

        await _repository.SaveChunksAsync(document.Id, new[]
        {
            BuildChunk(document.Id, "relevant content", closeEmbedding),
            BuildChunk(document.Id, "unrelated content", farEmbedding),
        }, CancellationToken.None);

        var results = await _repository.SearchAsync(
            new DocumentSearchQuery(closeEmbedding, Domain: null, Audience: null, SpaceKey: null, System: null, Limit: 5),
            CancellationToken.None);

        results.Should().HaveCount(2);
        results[0].Chunk.Content.Should().Be("relevant content");
        results[0].Score.Should().BeGreaterThan(results[1].Score);
    }

    [Fact]
    public async Task GetByUrlAsync_WhenDocumentDoesNotExist_ReturnsNull()
    {
        var result = await _repository.GetByUrlAsync("https://docs.local/missing", CancellationToken.None);

        result.Should().BeNull();
    }

    private static Document BuildDocument(string url, int version) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Test document",
        Url = url,
        Source = DocumentSource.Markdown,
        SpaceKey = "ENG",
        DocumentType = DocumentType.TechnicalDoc,
        Audience = Audience.Developers,
        System = "knowledge-ai",
        Version = version,
        UpdatedAt = DateTimeOffset.UtcNow,
    };

    private static DocumentChunk BuildChunk(Guid documentId, string content, float[] embedding) => new()
    {
        Id = Guid.NewGuid(),
        DocumentId = documentId,
        Content = content,
        Embedding = embedding,
        Domain = KnowledgeDomain.Technical,
        Metadata = new Dictionary<string, string>(),
        UpdatedAt = DateTimeOffset.UtcNow,
    };

    private static float[] BuildEmbedding(float seed)
    {
        var embedding = new float[1536];
        embedding[0] = seed;
        return embedding;
    }
}
