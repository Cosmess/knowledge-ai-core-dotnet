using KnowledgeAi.Application.Common.Interfaces;

namespace KnowledgeAi.IntegrationTests.Api;

public sealed class FakeEmbeddingProvider : IEmbeddingProvider
{
    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken) =>
        Task.FromResult(new float[1536]);
}

public sealed class FakeLlmProvider : ILlmProvider
{
    public string ProviderName => "fake";

    public Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken) =>
        Task.FromResult("fake answer");
}
