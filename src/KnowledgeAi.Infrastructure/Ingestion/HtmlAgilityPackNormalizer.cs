using HtmlAgilityPack;
using KnowledgeAi.Application.Common.Interfaces;

namespace KnowledgeAi.Infrastructure.Ingestion;

public sealed class HtmlAgilityPackNormalizer : IHtmlNormalizer
{
    public string ToPlainText(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var text = document.DocumentNode.InnerText;
        return System.Net.WebUtility.HtmlDecode(text).Trim();
    }
}
