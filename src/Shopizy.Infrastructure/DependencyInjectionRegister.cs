using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopizy.Infrastructure.DependencyInjection;

namespace Shopizy.Infrastructure;

[ExcludeFromCodeCoverage]
public static class DependencyInjectionRegister
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddCors(options =>
        {
            options.AddPolicy(
                name: "_myAllowSpecificOrigins",
                builder =>
                {
                    builder
                        .WithOrigins(Origins(configuration))
                        .WithHeaders(AllowedHeaders(configuration))
                        .WithMethods(AllowedMethods(configuration))
                        .AllowCredentials();
                }
            );
        });

        return services
            .AddHttpContextAccessor()
            .AddExternalServices(configuration)
            .AddAuthenticationInternal(configuration)
            .AddSecurity()
            .AddPersistence(configuration)
            .AddMessaging();
    }

    private static string[] Origins(IConfiguration configuration) =>
        configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? [];

    private static readonly string[] DefaultAllowedHeaders =
    [
        "Authorization",
        "Content-Type",
        "Accept",
        "Origin",
        "X-Requested-With",
        "Idempotency-Key",
    ];

    private static readonly string[] DefaultAllowedMethods =
    [
        "GET",
        "POST",
        "PUT",
        "PATCH",
        "DELETE",
        "OPTIONS",
    ];

    private static string[] AllowedHeaders(IConfiguration configuration) =>
        configuration.GetSection("CorsSettings:AllowedHeaders").Get<string[]>()
        ?? DefaultAllowedHeaders;

    private static string[] AllowedMethods(IConfiguration configuration) =>
        configuration.GetSection("CorsSettings:AllowedMethods").Get<string[]>()
        ?? DefaultAllowedMethods;
}
