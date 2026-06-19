using KnowledgeAi.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Prometheus;

namespace KnowledgeAi.Infrastructure.Observability;

public sealed class PrometheusLlmMetricsRecorder : ILlmMetricsRecorder
{
    private static readonly Counter TokensTotal = Metrics.CreateCounter(
        "llm_tokens_total", "Total LLM tokens consumed, by provider and direction.", "provider", "direction");

    private static readonly Counter CostTotal = Metrics.CreateCounter(
        "llm_cost_usd_total", "Estimated LLM cost in USD, by provider. Zero if LlmPricing is not configured for that provider.", "provider");

    private static readonly Counter FallbackTotal = Metrics.CreateCounter(
        "llm_fallback_total", "Number of times the extractive fallback was used because the LLM call failed.", "provider");

    private static readonly Counter EvidenceOutcomeTotal = Metrics.CreateCounter(
        "chat_evidence_outcome_total", "Number of chat/search requests by evidence outcome.", "outcome");

    private readonly LlmPricingOptions _pricing;

    public PrometheusLlmMetricsRecorder(IOptions<LlmPricingOptions> pricing)
    {
        _pricing = pricing.Value;
    }

    public void RecordCompletion(string provider, int? inputTokens, int? outputTokens, bool wasFallback)
    {
        if (wasFallback)
        {
            FallbackTotal.WithLabels(provider).Inc();
            return;
        }

        if (inputTokens is { } input)
        {
            TokensTotal.WithLabels(provider, "input").Inc(input);
        }

        if (outputTokens is { } output)
        {
            TokensTotal.WithLabels(provider, "output").Inc(output);
        }

        var cost = EstimateCost(provider, inputTokens, outputTokens);
        if (cost > 0)
        {
            CostTotal.WithLabels(provider).Inc(cost);
        }
    }

    public void RecordEvidenceOutcome(bool hasEnoughEvidence) =>
        EvidenceOutcomeTotal.WithLabels(hasEnoughEvidence ? "found" : "insufficient").Inc();

    private double EstimateCost(string provider, int? inputTokens, int? outputTokens)
    {
        if (!_pricing.Providers.TryGetValue(provider, out var rates))
        {
            return 0;
        }

        return (inputTokens ?? 0) / 1000d * rates.InputPricePerThousandTokens
            + (outputTokens ?? 0) / 1000d * rates.OutputPricePerThousandTokens;
    }
}
