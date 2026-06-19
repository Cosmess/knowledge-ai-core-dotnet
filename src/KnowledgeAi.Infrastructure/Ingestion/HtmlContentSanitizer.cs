using HtmlAgilityPack;
using KnowledgeAi.Application.Common.Interfaces;

namespace KnowledgeAi.Infrastructure.Ingestion;

/// <summary>
/// Strips executable/styling markup (script, style, iframe, object, embed, noscript) and
/// event-handler/javascript: attributes from ingested content before it reaches chunking.
/// This guards against markup injection landing in the LLM context; it does not address
/// natural-language prompt injection, which remains mitigated only by the chat system prompt rules.
/// </summary>
public sealed class HtmlContentSanitizer : IContentSanitizer
{
    private static readonly string[] DangerousTags = ["script", "style", "iframe", "object", "embed", "noscript"];

    public string Sanitize(string rawContent)
    {
        var document = new HtmlDocument();
        document.LoadHtml(rawContent);

        var dangerousNodes = document.DocumentNode.SelectNodes(
            string.Join('|', DangerousTags.Select(tag => $"//{tag}")));

        if (dangerousNodes is not null)
        {
            foreach (var node in dangerousNodes)
            {
                node.Remove();
            }
        }

        foreach (var node in document.DocumentNode.Descendants().ToList())
        {
            var dangerousAttributes = node.Attributes
                .Where(attribute =>
                    attribute.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase) ||
                    attribute.Value.TrimStart().StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var attribute in dangerousAttributes)
            {
                node.Attributes.Remove(attribute);
            }
        }

        return document.DocumentNode.OuterHtml;
    }
}
