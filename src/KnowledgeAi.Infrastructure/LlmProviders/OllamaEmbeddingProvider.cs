using KnowledgeAi.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using OllamaSharp;

namespace KnowledgeAi.Infrastructure.LlmProviders;

public sealed class OllamaEmbeddingProvider : IEmbeddingProvider
{
    private readonly IOllamaApiClient _client;

    public OllamaEmbeddingProvider(IOptions<LlmProviderOptions> options)
    {
        var settings = options.Value;
        _client = new OllamaApiClient(new Uri(settings.OllamaBaseUrl), settings.OllamaEmbeddingModel);
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken)
    {
        var response = await _client.EmbedAsync(text, cancellationToken);
        return response.Embeddings[0];
    }
}
