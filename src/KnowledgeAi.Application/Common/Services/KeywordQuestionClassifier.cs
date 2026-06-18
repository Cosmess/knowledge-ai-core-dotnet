using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Common.Services;

/// <summary>
/// Simple keyword-based classifier, mirroring the original rag-core domain classification.
/// Pure logic, no infrastructure dependency, so it lives in Application rather than Infrastructure.
/// </summary>
public sealed class KeywordQuestionClassifier : IQuestionClassifier
{
    private static readonly (KnowledgeDomain Domain, string[] Keywords)[] Rules =
    [
        (KnowledgeDomain.Business, ["regra", "negócio", "liquidacao", "liquidação", "contrato"]),
        (KnowledgeDomain.Operations, ["processo", "operacional", "runbook", "incidente"]),
        (KnowledgeDomain.Api, ["endpoint", "api", "rota", "request", "response"]),
        (KnowledgeDomain.Architecture, ["arquitetura", "componente", "decisão técnica"]),
        (KnowledgeDomain.Product, ["produto", "feature", "roadmap"])
    ];

    public KnowledgeDomain Classify(string question)
    {
        var lowered = question.ToLowerInvariant();

        foreach (var (domain, keywords) in Rules)
        {
            if (keywords.Any(lowered.Contains))
            {
                return domain;
            }
        }

        return KnowledgeDomain.Technical;
    }
}
