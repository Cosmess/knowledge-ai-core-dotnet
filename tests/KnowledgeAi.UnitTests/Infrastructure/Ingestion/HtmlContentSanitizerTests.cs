using FluentAssertions;
using KnowledgeAi.Infrastructure.Ingestion;

namespace KnowledgeAi.UnitTests.Infrastructure.Ingestion;

public class HtmlContentSanitizerTests
{
    private readonly HtmlContentSanitizer _sanitizer = new();

    [Fact]
    public void Sanitize_RemovesScriptTagsAndTheirContent()
    {
        var html = "<p>Before</p><script>alert('xss')</script><p>After</p>";

        var sanitized = _sanitizer.Sanitize(html);

        sanitized.Should().NotContain("script");
        sanitized.Should().NotContain("alert");
        sanitized.Should().Contain("Before");
        sanitized.Should().Contain("After");
    }

    [Fact]
    public void Sanitize_RemovesEventHandlerAttributes()
    {
        var html = "<img src=\"x.png\" onerror=\"alert('xss')\" />";

        var sanitized = _sanitizer.Sanitize(html);

        sanitized.Should().NotContain("onerror");
    }

    [Fact]
    public void Sanitize_RemovesJavascriptUrls()
    {
        var html = "<a href=\"javascript:alert('xss')\">click</a>";

        var sanitized = _sanitizer.Sanitize(html);

        sanitized.Should().NotContain("javascript:");
    }

    [Fact]
    public void Sanitize_LeavesNormalMarkdownAndHtmlUntouched()
    {
        var content = "# Title\n\nSome **bold** text and a <h2>Section</h2>.";

        var sanitized = _sanitizer.Sanitize(content);

        sanitized.Should().Contain("# Title");
        sanitized.Should().Contain("**bold**");
        sanitized.Should().Contain("<h2>Section</h2>");
    }
}
