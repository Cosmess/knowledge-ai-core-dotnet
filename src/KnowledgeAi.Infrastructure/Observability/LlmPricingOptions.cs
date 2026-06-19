namespace KnowledgeAi.Infrastructure.Observability;

/// <summary>
/// Optional, operator-configured $/1K-token rates per LLM provider, used only to estimate cost in metrics.
/// Left unconfigured, cost for that provider is reported as 0 rather than a guessed/stale number.
/// </summary>
public sealed class LlmPricingOptions
{
    public const string SectionName = "LlmPricing";

    public Dictionary<string, ProviderRates> Providers { get; set; } = new();

    public sealed class ProviderRates
    {
        public double InputPricePerThousandTokens { get; set; }

        public double OutputPricePerThousandTokens { get; set; }
    }
}
