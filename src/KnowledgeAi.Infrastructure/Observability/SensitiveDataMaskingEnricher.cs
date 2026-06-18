using Serilog.Core;
using Serilog.Events;

namespace KnowledgeAi.Infrastructure.Observability;

/// <summary>
/// Masks sensitive log properties by default for every log event, instead of relying on call
/// sites to remember to mask. Fixes the original gap where a masking helper existed but was
/// never wired into the actual logging pipeline.
/// </summary>
public sealed class SensitiveDataMaskingEnricher : ILogEventEnricher
{
    private const string MaskedValue = "***MASKED***";

    private static readonly string[] SensitiveKeyFragments =
    [
        "password", "token", "apikey", "api_key", "secret", "accesstoken", "refreshtoken", "authorization"
    ];

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var property in logEvent.Properties.ToList())
        {
            if (IsSensitive(property.Key))
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(property.Key, MaskedValue));
            }
        }
    }

    private static bool IsSensitive(string key) =>
        SensitiveKeyFragments.Any(fragment => key.Contains(fragment, StringComparison.OrdinalIgnoreCase));
}
