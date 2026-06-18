namespace KnowledgeAi.Application.Common.Interfaces;

public interface ILlmProvider
{
    string ProviderName { get; }

    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken);
}
