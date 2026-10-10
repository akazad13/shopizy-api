using System.Diagnostics.CodeAnalysis;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Shopizy.Api.Common.Telemetry;

[ExcludeFromCodeCoverage]
public static class TelemetryRegister
{
    public const string ServiceName = "Shopizy.Api";
    public const string OutboxMeterName = "Shopizy.Outbox";

    public static IServiceCollection AddTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        var resource = ResourceBuilder
            .CreateDefault()
            .AddService(
                ServiceName,
                serviceVersion: typeof(TelemetryRegister).Assembly.GetName().Version?.ToString()
                    ?? "1.0.0"
            )
            .AddAttributes(
                new KeyValuePair<string, object>[]
                {
                    new("deployment.environment", environment.EnvironmentName),
                }
            );

        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        services
            .AddOpenTelemetry()
            .ConfigureResource(_ => _.AddService(ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.Filter = ctx => ctx.Request.Path != "/healthz";
                    })
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddRedisInstrumentation();

                ConfigureExporter(tracing, otlpEndpoint, environment);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resource)
                    .AddMeter(OutboxMeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                ConfigureMeterExporter(metrics, otlpEndpoint, environment);
            });

        return services;
    }

    private static void ConfigureExporter(
        TracerProviderBuilder tracing,
        string? otlpEndpoint,
        IHostEnvironment environment
    )
    {
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }
        else if (environment.IsDevelopment())
        {
            tracing.AddConsoleExporter();
        }
    }

    private static void ConfigureMeterExporter(
        MeterProviderBuilder metrics,
        string? otlpEndpoint,
        IHostEnvironment environment
    )
    {
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }
        else if (environment.IsDevelopment())
        {
            metrics.AddConsoleExporter();
        }
    }
}
