namespace KnowledgeAi.Application.Common.Interfaces;

public sealed record LlmCompletionResult(string Text, int? InputTokens, int? OutputTokens);

public interface ILlmProvider
{
    string ProviderName { get; }

    Task<LlmCompletionResult> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken);
}
