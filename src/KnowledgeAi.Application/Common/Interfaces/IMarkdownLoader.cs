namespace KnowledgeAi.Application.Common.Interfaces;

public sealed record MarkdownDocument(
    string Title,
    string Content,
    string FilePath,
    IReadOnlyDictionary<string, string> Frontmatter);

public interface IMarkdownLoader
{
    Task<IReadOnlyList<MarkdownDocument>> LoadAsync(string rootDir, CancellationToken cancellationToken);
}
