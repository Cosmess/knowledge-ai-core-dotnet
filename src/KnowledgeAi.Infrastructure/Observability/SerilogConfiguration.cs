using Serilog;

namespace KnowledgeAi.Infrastructure.Observability;

public static class SerilogConfiguration
{
    /// <summary>
    /// Applies the project-wide Serilog setup. The masking enricher runs for every sink,
    /// so call sites cannot accidentally log secrets by forgetting to mask them locally.
    /// </summary>
    public static LoggerConfiguration Configure(LoggerConfiguration configuration) =>
        configuration
            .Enrich.With<SensitiveDataMaskingEnricher>()
            .WriteTo.Console();
}
