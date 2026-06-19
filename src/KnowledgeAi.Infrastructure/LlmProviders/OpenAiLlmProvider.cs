using KnowledgeAi.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace KnowledgeAi.Infrastructure.LlmProviders;

public sealed class OpenAiLlmProvider : ILlmProvider
{
    private readonly ChatClient _chatClient;

    public string ProviderName => "openai";

    public OpenAiLlmProvider(IOptions<LlmProviderOptions> options)
    {
        var settings = options.Value;
        _chatClient = new ChatClient(settings.OpenAiChatModel, settings.OpenAiApiKey);
    }

    public async Task<LlmCompletionResult> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        ChatMessage[] messages = [new SystemChatMessage(systemPrompt), new UserChatMessage(userPrompt)];
        var completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        var usage = completion.Value.Usage;
        return new LlmCompletionResult(completion.Value.Content[0].Text, usage?.InputTokenCount, usage?.OutputTokenCount);
    }
}
