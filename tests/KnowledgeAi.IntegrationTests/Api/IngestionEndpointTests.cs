using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using KnowledgeAi.Application.Ingestion.Commands;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.IntegrationTests.Api;

public sealed class IngestionEndpointTests : IClassFixture<KnowledgeApiFactory>, IDisposable
{
    private readonly KnowledgeApiFactory _factory;
    private readonly string _rootDir;

    public IngestionEndpointTests(KnowledgeApiFactory factory)
    {
        _factory = factory;
        _rootDir = Path.Combine(Path.GetTempPath(), "kai-ingest-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(_rootDir);
    }

    public void Dispose() => Directory.Delete(_rootDir, recursive: true);

    [Fact]
    public async Task IngestMarkdown_WithDeveloperRole_ReturnsForbidden()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _factory.CreateAccessTokenFor(Role.Developer, "ENG"));

        var response = await client.PostAsJsonAsync(
            "ingest/markdown", new IngestMarkdownCommand(_rootDir, "ENG"), ApiJsonOptions.Default);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task IngestMarkdown_WithAdminRole_IngestsMarkdownFilesFromDisk()
    {
        await File.WriteAllTextAsync(Path.Combine(_rootDir, "getting-started.md"), """
            ---
            documentType: technicalDoc
            audience: developers
            system: knowledge-ai
            ---
            # Getting Started

            This is the introductory section describing how to set up the project locally for development.
            """);

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _factory.CreateAccessTokenFor(Role.Admin, "ENG"));

        var response = await client.PostAsJsonAsync(
            "ingest/markdown", new IngestMarkdownCommand(_rootDir, "ENG"), ApiJsonOptions.Default);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<IngestionResult>(ApiJsonOptions.Default);
        result!.DocumentsProcessed.Should().Be(1);
        result.ChunksProcessed.Should().BeGreaterThan(0);
    }
}
