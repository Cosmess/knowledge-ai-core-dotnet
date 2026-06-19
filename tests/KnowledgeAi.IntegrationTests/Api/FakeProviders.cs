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

    public Task<LlmCompletionResult> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken) =>
        Task.FromResult(new LlmCompletionResult("fake answer", InputTokens: 10, OutputTokens: 5));
}
