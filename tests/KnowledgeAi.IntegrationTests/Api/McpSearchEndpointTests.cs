using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using KnowledgeAi.Api.Auth;
using KnowledgeAi.Api.Contracts;
using KnowledgeAi.Application.Search.Queries;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.IntegrationTests.Api;

public sealed class McpSearchEndpointTests : IClassFixture<KnowledgeApiFactory>
{
    private readonly KnowledgeApiFactory _factory;

    public McpSearchEndpointTests(KnowledgeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Search_WithoutApiKey_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _factory.CreateAccessTokenFor(Role.Developer, "ENG"));

        var response = await client.PostAsJsonAsync(
            "mcp/search",
            new McpSearchRequest("how does auth work", KnowledgeDomain.Technical, Audience.Developers, "ENG", "knowledge-ai", null),
            ApiJsonOptions.Default);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Search_WithApiKeyButNoUserToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyDefaults.HeaderName, KnowledgeApiFactory.ApiKeyValue);

        var response = await client.PostAsJsonAsync(
            "mcp/search",
            new McpSearchRequest("how does auth work", KnowledgeDomain.Technical, Audience.Developers, "ENG", "knowledge-ai", null),
            ApiJsonOptions.Default);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Search_WithValidApiKeyAndUserToken_ReturnsInsufficientEvidence()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyDefaults.HeaderName, KnowledgeApiFactory.ApiKeyValue);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _factory.CreateAccessTokenFor(Role.Developer, "ENG"));

        var response = await client.PostAsJsonAsync(
            "mcp/search",
            new McpSearchRequest("how does auth work", KnowledgeDomain.Technical, Audience.Developers, "ENG", "knowledge-ai", null),
            ApiJsonOptions.Default);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<McpSearchResult>(ApiJsonOptions.Default);
        result!.EvidenceStatus.Should().Be(EvidenceStatus.Insufficient);
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_WithSpaceKeyNotAllowedForUser_ReturnsForbidden()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyDefaults.HeaderName, KnowledgeApiFactory.ApiKeyValue);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _factory.CreateAccessTokenFor(Role.Developer, "ENG"));

        var response = await client.PostAsJsonAsync(
            "mcp/search",
            new McpSearchRequest("how does auth work", KnowledgeDomain.Technical, Audience.Developers, "OTHER", "knowledge-ai", null),
            ApiJsonOptions.Default);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
