using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace KnowledgeAi.Infrastructure.Observability;

public static class OpenTelemetryExtensions
{
    private const string ServiceName = "knowledge-ai-core";

    public static IServiceCollection AddKnowledgeAiObservability(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddSource(ServiceName)
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter());

        return services;
    }
}
