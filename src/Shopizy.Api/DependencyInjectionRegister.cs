using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Shopizy.Api.Common.Errors;
using Shopizy.Api.Common.Idempotency;
using Shopizy.Api.Common.Mapping;

namespace Shopizy.Api;

/// <summary>
/// Dependency injection register for presentation layer.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DependencyInjectionRegister
{
    /// <summary>
    /// Adds presentation layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection with presentation layer services added.</returns>
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("api-version")
            );
        });

        services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
                var xmlFile =
                    $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                var contractsXmlFile = "Shopizy.Contracts.xml";
                var contractsXmlPath = Path.Combine(AppContext.BaseDirectory, contractsXmlFile);
                options.IncludeXmlComments(contractsXmlPath);
            });
        services.AddMappings();

        services.AddScoped<IdempotencyEndpointFilter>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddHealthChecks();

        services.AddRateLimiter(options =>
        {
            // Fixed window for auth endpoints
            options.AddFixedWindowLimiter(
                "auth",
                limiter =>
                {
                    limiter.Window = TimeSpan.FromMinutes(1);
                    limiter.PermitLimit = 10000;
                    limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiter.QueueLimit = 0;
                }
            );

            // Sliding window for general API
            options.AddSlidingWindowLimiter(
                "api",
                limiter =>
                {
                    limiter.Window = TimeSpan.FromMinutes(1);
                    limiter.SegmentsPerWindow = 6;
                    limiter.PermitLimit = 100000;
                    limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiter.QueueLimit = 0;
                }
            );

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}
