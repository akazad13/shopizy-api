using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Infrastructure.AuditLogs.Persistence;
using Shopizy.Infrastructure.Brands.Persistence;
using Shopizy.Infrastructure.Carts.Persistence;
using Shopizy.Infrastructure.Categories.Persistence;
using Shopizy.Infrastructure.Common.Caching;
using Shopizy.Infrastructure.Common.HealthChecks;
using Shopizy.Infrastructure.Common.Persistence;
using Shopizy.Infrastructure.Common.Persistence.Interceptors;
using Shopizy.Infrastructure.GiftCards.Persistence;
using Shopizy.Infrastructure.LoyaltyAccounts.Persistence;
using Shopizy.Infrastructure.Orders.Persistence;
using Shopizy.Infrastructure.Outbox;
using Shopizy.Infrastructure.Payments.Persistence;
using Shopizy.Infrastructure.Permissions.Persistence;
using Shopizy.Infrastructure.ProductQuestions.Persistence;
using Shopizy.Infrastructure.ProductReviews.Persistence;
using Shopizy.Infrastructure.Products.Persistence;
using Shopizy.Infrastructure.PromoCodes.Persistence;
using Shopizy.Infrastructure.Returns.Persistence;
using Shopizy.Infrastructure.Services;
using Shopizy.Infrastructure.Users.Persistence;
using Shopizy.Infrastructure.Wishlists.Persistence;
using Shopizy.SharedKernel.Application.Interfaces.Persistence;
using StackExchange.Redis;

namespace Shopizy.Infrastructure.DependencyInjection;

public static class PersistenceRegister
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null
    )
    {
        services
            .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>())
            .AddTransient<DbMigrationsHelper>()
            .AddScoped<UpdateAuditableEntitiesInterceptor>()
            .AddSingleton<IPermissionLookup, PermissionLookup>();

        var isTesting =
            (
                environment?.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase)
                == true
            )
            || string.Equals(
                configuration["ASPNETCORE_ENVIRONMENT"],
                "Testing",
                StringComparison.OrdinalIgnoreCase
            )
            || string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Testing",
                StringComparison.OrdinalIgnoreCase
            );

        var redisSettings =
            configuration.GetSection(RedisSettings.Section).Get<RedisSettings>()
            ?? new RedisSettings();

        var useRedis = !isTesting && !string.IsNullOrWhiteSpace(redisSettings.Endpoint);

        services.AddLogging();
        services.AddMemoryCache();
        services.AddEFSecondLevelCache(options =>
        {
            if (!useRedis)
            {
                options.UseMemoryCacheProvider().UseCacheKeyPrefix("EF_");
            }
            else
            {
                var redisOptions = new ConfigurationOptions
                {
                    EndPoints = { { redisSettings.Endpoint, redisSettings.Port } },
                    User = string.IsNullOrWhiteSpace(redisSettings.Username)
                        ? null
                        : redisSettings.Username,
                    Password = string.IsNullOrWhiteSpace(redisSettings.Password)
                        ? null
                        : redisSettings.Password,
                    AbortOnConnectFail = false,
                    ConnectTimeout = 5000,
                    SyncTimeout = 5000,
                };

                options
                    .UseStackExchangeRedisCacheProvider(redisOptions, TimeSpan.FromMinutes(5))
                    .UseCacheKeyPrefix("EF_")
                    .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1));
            }
        });

        services.AddDbContext<AppDbContext>(
            (sp, options) =>
            {
                var auditableInterceptor =
                    sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>();
                var secondLevelCacheInterceptor =
                    sp.GetRequiredService<SecondLevelCacheInterceptor>();

                options
                    .UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        o =>
                        {
                            o.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null
                            );
                            o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        }
                    )
                    .AddInterceptors(auditableInterceptor, secondLevelCacheInterceptor)
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }
        );

        services
            .AddHealthChecks()
            .AddCheck<DbHealthCheck>("database")
            .AddCheck<OutboxHealthCheck>("outbox");
        services.AddHostedService<DbMigrationsHostedService>();
        services.AddHostedService<OutboxProcessor>();
        services.AddScoped<IOutboxDrainer, OutboxDrainer>();

        services.Configure<Shopizy.Infrastructure.Orders.OrderSettings>(
            configuration.GetSection(Shopizy.Infrastructure.Orders.OrderSettings.Section)
        );
        services.AddHostedService<Shopizy.Infrastructure.Orders.Services.PendingOrderExpirationWorker>();

        services.Configure<Shopizy.Infrastructure.Carts.CartSettings>(
            configuration.GetSection(Shopizy.Infrastructure.Carts.CartSettings.Section)
        );
        services.AddHostedService<Shopizy.Infrastructure.Carts.Services.AbandonedCartReminderWorker>();

        return services.AddRepositories();
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services
            .AddScoped<IBrandRepository, BrandRepository>()
            .AddScoped<ICategoryRepository, CategoryRepository>()
            .AddScoped<ICartRepository, CartRepository>()
            .AddScoped<IOrderRepository, OrderRepository>()
            .AddScoped<IOrderReader, OrderReader>()
            .AddScoped<IPaymentRepository, PaymentRepository>()
            .AddScoped<IProductReviewRepository, ProductReviewRepository>()
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IProductReader, ProductReader>()
            .AddScoped<IPromoCodeRepository, PromoCodeRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IPermissionRepository, PermissionRepository>()
            .AddScoped<IWishlistRepository, WishlistRepository>()
            .AddScoped<ILoyaltyAccountRepository, LoyaltyAccountRepository>()
            .AddScoped<IGiftCardRepository, GiftCardRepository>()
            .AddScoped<IProductQuestionRepository, ProductQuestionRepository>()
            .AddScoped<IAuditLogRepository, AuditLogRepository>()
            .AddScoped<IReturnRequestRepository, ReturnRequestRepository>();

        return services;
    }
}
