using KnowledgeAi.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;

namespace KnowledgeAi.Infrastructure.LlmProviders;

public sealed class OpenAiEmbeddingProvider : IEmbeddingProvider
{
    private readonly EmbeddingClient _embeddingClient;

    public OpenAiEmbeddingProvider(IOptions<LlmProviderOptions> options)
    {
        var settings = options.Value;
        _embeddingClient = new EmbeddingClient(settings.OpenAiEmbeddingModel, settings.OpenAiApiKey);
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken)
    {
        var result = await _embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return result.Value.ToFloats().ToArray();
    }
}
