using System.Text;
using System.Text.RegularExpressions;
using KnowledgeAi.Application.Common.Interfaces;

namespace KnowledgeAi.Infrastructure.Ingestion;

/// <summary>
/// Mirrors docs/rag/chunking.md: split by heading, merge small sections, split large sections by word budget.
/// </summary>
public sealed partial class HeadingChunkingService : IChunkingService
{
    private const int WordBudget = 450;
    private const int MinWordsPerSection = 30;

    public IReadOnlyList<TextChunk> Split(string content)
    {
        var sections = MergeSmallSections(SplitByHeading(content));
        var chunks = new List<TextChunk>();

        foreach (var (headingPath, sectionContent) in sections)
        {
            var trimmed = sectionContent.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length <= WordBudget)
            {
                chunks.Add(new TextChunk(trimmed, headingPath));
                continue;
            }

            for (var offset = 0; offset < words.Length; offset += WordBudget)
            {
                var slice = string.Join(' ', words.Skip(offset).Take(WordBudget));
                chunks.Add(new TextChunk(slice, headingPath));
            }
        }

        return chunks;
    }

    private static List<(string? HeadingPath, string Content)> SplitByHeading(string content)
    {
        var sections = new List<(string? HeadingPath, string Content)>();
        var headingStack = new List<string>();
        var buffer = new StringBuilder();

        void Flush()
        {
            if (buffer.Length > 0)
            {
                sections.Add((headingStack.Count == 0 ? null : string.Join(" > ", headingStack), buffer.ToString()));
                buffer.Clear();
            }
        }

        foreach (var line in content.Split('\n'))
        {
            var match = HeadingPattern().Match(line);
            if (match.Success)
            {
                Flush();
                var level = match.Groups[1].Value.Length;

                while (headingStack.Count >= level)
                {
                    headingStack.RemoveAt(headingStack.Count - 1);
                }

                headingStack.Add(match.Groups[2].Value.Trim());
                continue;
            }

            buffer.AppendLine(line);
        }

        Flush();
        return sections;
    }

    private static List<(string? HeadingPath, string Content)> MergeSmallSections(List<(string? HeadingPath, string Content)> sections)
    {
        var merged = new List<(string? HeadingPath, string Content)>();

        foreach (var section in sections)
        {
            var wordCount = section.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            if (merged.Count > 0 && wordCount < MinWordsPerSection)
            {
                var last = merged[^1];
                merged[^1] = (last.HeadingPath, last.Content + "\n" + section.Content);
            }
            else
            {
                merged.Add(section);
            }
        }

        return merged;
    }

    [GeneratedRegex(@"^(#{1,6})\s+(.*)$")]
    private static partial Regex HeadingPattern();
}
