using System.Text;
using KnowledgeAi.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using OllamaSharp;

namespace KnowledgeAi.Infrastructure.LlmProviders;

public sealed class OllamaLlmProvider : ILlmProvider
{
    private readonly IOllamaApiClient _client;

    public string ProviderName => "ollama";

    public OllamaLlmProvider(IOptions<LlmProviderOptions> options)
    {
        var settings = options.Value;
        _client = new OllamaApiClient(new Uri(settings.OllamaBaseUrl), settings.OllamaChatModel);
    }

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        var chat = new Chat(_client, systemPrompt);
        var builder = new StringBuilder();

        await foreach (var chunk in chat.SendAsync(userPrompt, cancellationToken))
        {
            builder.Append(chunk);
        }

        return builder.ToString();
    }
}
