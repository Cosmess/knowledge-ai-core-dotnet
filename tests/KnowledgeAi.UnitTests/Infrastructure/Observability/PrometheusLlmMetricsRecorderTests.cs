using FluentAssertions;
using KnowledgeAi.Infrastructure.Observability;
using Microsoft.Extensions.Options;

namespace KnowledgeAi.UnitTests.Infrastructure.Observability;

public class PrometheusLlmMetricsRecorderTests
{
    [Fact]
    public void RecordCompletion_WithConfiguredPricing_DoesNotThrow()
    {
        var pricing = new LlmPricingOptions
        {
            Providers = new Dictionary<string, LlmPricingOptions.ProviderRates>
            {
                ["test-provider-priced"] = new() { InputPricePerThousandTokens = 1.0, OutputPricePerThousandTokens = 2.0 },
            },
        };
        var recorder = new PrometheusLlmMetricsRecorder(Options.Create(pricing));

        var act = () => recorder.RecordCompletion("test-provider-priced", inputTokens: 1000, outputTokens: 500, wasFallback: false);

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordCompletion_WithoutConfiguredPricing_DoesNotThrow()
    {
        var recorder = new PrometheusLlmMetricsRecorder(Options.Create(new LlmPricingOptions()));

        var act = () => recorder.RecordCompletion("test-provider-unpriced", inputTokens: 100, outputTokens: 50, wasFallback: false);

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordCompletion_WhenFallback_DoesNotThrow()
    {
        var recorder = new PrometheusLlmMetricsRecorder(Options.Create(new LlmPricingOptions()));

        var act = () => recorder.RecordCompletion("test-provider-fallback", inputTokens: null, outputTokens: null, wasFallback: true);

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordEvidenceOutcome_DoesNotThrow()
    {
        var recorder = new PrometheusLlmMetricsRecorder(Options.Create(new LlmPricingOptions()));

        var act = () =>
        {
            recorder.RecordEvidenceOutcome(hasEnoughEvidence: true);
            recorder.RecordEvidenceOutcome(hasEnoughEvidence: false);
        };

        act.Should().NotThrow();
    }
}
