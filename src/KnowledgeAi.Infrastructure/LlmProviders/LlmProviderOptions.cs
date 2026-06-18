namespace KnowledgeAi.Infrastructure.LlmProviders;

public enum ChatProviderKind
{
    OpenAi,
    Anthropic,
    Ollama
}

public enum EmbeddingProviderKind
{
    OpenAi,
    Ollama
}

public sealed class LlmProviderOptions
{
    public const string SectionName = "LlmProviders";

    public ChatProviderKind ChatProvider { get; set; } = ChatProviderKind.OpenAi;
    public EmbeddingProviderKind EmbeddingProvider { get; set; } = EmbeddingProviderKind.OpenAi;

    public string? OpenAiApiKey { get; set; }
    public string OpenAiChatModel { get; set; } = "gpt-4o-mini";
    public string OpenAiEmbeddingModel { get; set; } = "text-embedding-3-small";

    public string? AnthropicApiKey { get; set; }
    public string AnthropicChatModel { get; set; } = "claude-3-5-sonnet-20241022";

    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";
    public string OllamaChatModel { get; set; } = "llama3.1";
    public string OllamaEmbeddingModel { get; set; } = "nomic-embed-text";
}
