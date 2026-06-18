namespace KnowledgeAi.Application.Common.Interfaces;

public interface IEmbeddingProvider
{
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken);
}
