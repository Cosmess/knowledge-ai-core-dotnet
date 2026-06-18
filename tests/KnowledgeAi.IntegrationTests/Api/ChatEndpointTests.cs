using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using KnowledgeAi.Application.Chat.Commands;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.IntegrationTests.Api;

public sealed class ChatEndpointTests : IClassFixture<KnowledgeApiFactory>
{
    private readonly KnowledgeApiFactory _factory;

    public ChatEndpointTests(KnowledgeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Ask_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "chat", new AskQuestionCommand("How do I deploy?", Audience.Developers, "ENG", "knowledge-ai"), ApiJsonOptions.Default);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Ask_WhenNoDocumentsAreIngested_ReturnsInsufficientEvidence()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _factory.CreateAccessTokenFor(Role.Developer, "ENG"));

        var response = await client.PostAsJsonAsync(
            "chat", new AskQuestionCommand("How do I deploy this service?", Audience.Developers, "ENG", "knowledge-ai"), ApiJsonOptions.Default);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AskQuestionResult>(ApiJsonOptions.Default);
        result!.EvidenceStatus.Should().Be(EvidenceStatus.Insufficient);
        result.Confidence.Should().Be(0);
    }
}
