using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using LoadedMarkdownDocument = KnowledgeAi.Application.Common.Interfaces.MarkdownDocument;
using IMarkdownLoader = KnowledgeAi.Application.Common.Interfaces.IMarkdownLoader;

namespace KnowledgeAi.Infrastructure.Ingestion;

public sealed class MarkdigMarkdownLoader : IMarkdownLoader
{
    public Task<IReadOnlyList<LoadedMarkdownDocument>> LoadAsync(string rootDir, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(rootDir))
        {
            return Task.FromResult<IReadOnlyList<LoadedMarkdownDocument>>([]);
        }

        var documents = new List<LoadedMarkdownDocument>();

        foreach (var filePath in Directory.EnumerateFiles(rootDir, "*.md", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var raw = File.ReadAllText(filePath);
            var (frontmatter, body) = ExtractFrontmatter(raw);
            var title = ExtractTitle(body) ?? Path.GetFileNameWithoutExtension(filePath);

            documents.Add(new LoadedMarkdownDocument(title, body, filePath, frontmatter));
        }

        return Task.FromResult<IReadOnlyList<LoadedMarkdownDocument>>(documents);
    }

    private static (IReadOnlyDictionary<string, string> Frontmatter, string Body) ExtractFrontmatter(string raw)
    {
        if (!raw.StartsWith("---", StringComparison.Ordinal))
        {
            return (new Dictionary<string, string>(), raw);
        }

        var endIndex = raw.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (endIndex < 0)
        {
            return (new Dictionary<string, string>(), raw);
        }

        var frontmatterBlock = raw[3..endIndex];
        var body = raw[(endIndex + 4)..].TrimStart('\n', '\r');

        var frontmatter = new Dictionary<string, string>();
        foreach (var line in frontmatterBlock.Split('\n'))
        {
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim().Trim('"', '\'');

            if (key.Length > 0)
            {
                frontmatter[key] = value;
            }
        }

        return (frontmatter, body);
    }

    private static string? ExtractTitle(string markdown)
    {
        var document = Markdown.Parse(markdown);
        var heading = document.Descendants<HeadingBlock>().FirstOrDefault(h => h.Level == 1);

        if (heading?.Inline is null)
        {
            return null;
        }

        var text = string.Concat(heading.Inline.Descendants<LiteralInline>().Select(literal => literal.Content.ToString()));
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }
}
