using FluentAssertions;
using KnowledgeAi.Infrastructure.Ingestion;

namespace KnowledgeAi.UnitTests.Infrastructure.Ingestion;

public class HtmlAgilityPackNormalizerTests
{
    private readonly HtmlAgilityPackNormalizer _normalizer = new();

    [Fact]
    public void ToPlainText_ConvertsHeadingTagsToMarkdownStyleHeadings()
    {
        var html = "<h2>Section</h2><p>Body text</p>";

        var text = _normalizer.ToPlainText(html);

        text.Should().Contain("## Section");
        text.Should().Contain("Body text");
    }

    [Fact]
    public void ToPlainText_PreservesHeadingsThroughChunking()
    {
        var html = "<h1>Title</h1><p>" + string.Join(" ", Enumerable.Repeat("intro", 35)) + "</p>";

        var text = _normalizer.ToPlainText(html);
        var chunks = new HeadingChunkingService().Split(text);

        chunks.Should().ContainSingle(chunk => chunk.HeadingPath == "Title");
    }
}
