using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using KnowledgeAi.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace KnowledgeAi.Infrastructure.LlmProviders;

public sealed class AnthropicLlmProvider : ILlmProvider
{
    private readonly AnthropicClient _client;
    private readonly string _model;

    public string ProviderName => "anthropic";

    public AnthropicLlmProvider(IOptions<LlmProviderOptions> options)
    {
        var settings = options.Value;
        _client = new AnthropicClient(new APIAuthentication(settings.AnthropicApiKey));
        _model = settings.AnthropicChatModel;
    }

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        var parameters = new MessageParameters
        {
            Model = _model,
            MaxTokens = 1024,
            Messages = [new Message(RoleType.User, userPrompt)],
            System = [new SystemMessage(systemPrompt)],
        };

        var response = await _client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);
        return response.FirstMessage.Text;
    }
}
