using HtmlAgilityPack;
using KnowledgeAi.Application.Common.Interfaces;

namespace KnowledgeAi.Infrastructure.Ingestion;

public sealed class HtmlAgilityPackNormalizer : IHtmlNormalizer
{
    public string ToPlainText(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        // HeadingChunkingService only recognizes Markdown-style "#" headings. Without this rewrite,
        // InnerText below would silently drop every <h1>-<h6> tag, leaving Confluence pages with no heading path.
        var headingNodes = document.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6");
        if (headingNodes is not null)
        {
            foreach (var heading in headingNodes)
            {
                var level = int.Parse(heading.Name[1..]);
                heading.InnerHtml = $"\n{new string('#', level)} {heading.InnerText}\n";
            }
        }

        var text = document.DocumentNode.InnerText;
        return System.Net.WebUtility.HtmlDecode(text).Trim();
    }
}
