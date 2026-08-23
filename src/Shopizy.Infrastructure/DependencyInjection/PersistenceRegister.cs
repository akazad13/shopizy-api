using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Infrastructure.AuditLogs.Persistence;
using Shopizy.Infrastructure.Brands.Persistence;
using Shopizy.Infrastructure.Carts.Persistence;
using Shopizy.Infrastructure.Categories.Persistence;
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

namespace Shopizy.Infrastructure.DependencyInjection;

public static class PersistenceRegister
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>())
            .AddTransient<DbMigrationsHelper>()
            .AddScoped<UpdateAuditableEntitiesInterceptor>()
            .AddSingleton<IPermissionLookup, PermissionLookup>();

        services.AddDbContext<AppDbContext>(
            (sp, options) =>
            {
                var interceptor = sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>();
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
                    .AddInterceptors(interceptor);
            }
        );

        services.AddHealthChecks().AddCheck<DbHealthCheck>("database");
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
