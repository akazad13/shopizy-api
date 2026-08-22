using System.Threading.RateLimiting;
using ErrorOr;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shopizy.Application.Common.Interfaces.Authentication;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Application.Products.Common;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.Infrastructure.Common.Persistence;
using Shopizy.Infrastructure.Common.Persistence.Interceptors;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Models;
using Testcontainers.MsSql;
using Xunit;

namespace Shopizy.Api.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly MsSqlContainer SharedDbContainer = new MsSqlBuilder(
        "mcr.microsoft.com/mssql/server:2022-latest"
    ).Build();

    private static readonly SemaphoreSlim ContainerLock = new(1, 1);
    private static bool ContainerStarted;

    private readonly string _databaseName = $"Shopizy_Integration_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(
            (context, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["JwtSettings:Secret"] = "super-secret-key-that-is-at-least-32-chars-long",
                        ["JwtSettings:Issuer"] = "Shopizy",
                        ["JwtSettings:Audience"] = "Shopizy",
                        ["JwtSettings:TokenExpirationMinutes"] = "60",
                        ["RedisCacheSettings:Endpoint"] = "localhost",
                        ["RedisCacheSettings:Port"] = "6379",
                    }
                );
            }
        );

        builder.ConfigureTestServices(services =>
        {
            ArgumentNullException.ThrowIfNull(services);
            // Remove the existing DbContext registration (if any)
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            services.AddDbContext<AppDbContext>(
                (sp, options) =>
                {
                    var interceptor = sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>();
                    var connectionString = BuildConnectionString();
                    options
                        .UseSqlServer(
                            connectionString,
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
                        .ConfigureWarnings(w =>
                            w.Ignore(
                                Microsoft
                                    .EntityFrameworkCore
                                    .Diagnostics
                                    .RelationalEventId
                                    .PendingModelChangesWarning
                            )
                        )
                        .AddInterceptors(interceptor);
                }
            );

            // Override JWT validation for testing
            services.PostConfigure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
                Microsoft
                    .AspNetCore
                    .Authentication
                    .JwtBearer
                    .JwtBearerDefaults
                    .AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters.ValidateIssuer = false;
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                    options.RequireHttpsMetadata = false;
                }
            );

            // Replace Redis cache with In-Memory stub
            services.RemoveAll(typeof(ICacheHelper));
            services.AddSingleton<ICacheHelper, InMemoryCacheHelper>();

            // Replace Redis idempotency store with an in-memory one so dedup behaviour can be exercised
            services.RemoveAll(typeof(IIdempotencyStore));
            services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

            // Replace Redis refresh-token store with in-memory implementation
            services.RemoveAll(typeof(IRefreshTokenStore));
            services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();

            // Mock IPaymentService
            services.RemoveAll(typeof(IPaymentService));
            services.AddScoped<IPaymentService, MockPaymentService>();

            // Mock IMediaUploader
            services.RemoveAll(typeof(IMediaUploader));
            services.AddScoped<IMediaUploader, MockMediaUploader>();

            // Disable rate limiting for tests: remove original Configure callbacks,
            // then re-register with very permissive limits so 429s never occur.
            services.RemoveAll<IConfigureOptions<RateLimiterOptions>>();
            services.Configure<RateLimiterOptions>(options =>
            {
                options.AddFixedWindowLimiter(
                    "auth",
                    o =>
                    {
                        o.Window = TimeSpan.FromSeconds(1);
                        o.PermitLimit = 10000;
                        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        o.QueueLimit = 0;
                    }
                );
                options.AddSlidingWindowLimiter(
                    "api",
                    o =>
                    {
                        o.Window = TimeSpan.FromSeconds(1);
                        o.SegmentsPerWindow = 1;
                        o.PermitLimit = 10000;
                        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        o.QueueLimit = 0;
                    }
                );
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });
        });
    }

    public class InMemoryRefreshTokenStore : IRefreshTokenStore
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<
            string,
            UserId
        > _tokens = new(StringComparer.Ordinal);

        public Task StoreAsync(
            string token,
            UserId userId,
            TimeSpan ttl,
            CancellationToken cancellationToken = default
        )
        {
            _tokens[token] = userId;
            return Task.CompletedTask;
        }

        public Task<UserId?> ConsumeAsync(
            string token,
            CancellationToken cancellationToken = default
        )
        {
            _tokens.TryRemove(token, out var userId);
            return Task.FromResult(userId);
        }

        public Task RevokeAsync(string token, CancellationToken cancellationToken = default)
        {
            _tokens.TryRemove(token, out _);
            return Task.CompletedTask;
        }

        public Task RevokeAllForUserAsync(
            UserId userId,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentNullException.ThrowIfNull(userId);
            foreach (var entry in _tokens.Where(kvp => kvp.Value.Value == userId.Value).ToList())
            {
                _tokens.TryRemove(entry.Key, out _);
            }
            return Task.CompletedTask;
        }
    }

    public class InMemoryIdempotencyStore : IIdempotencyStore
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<
            string,
            IdempotencyRecord
        > _records = new(StringComparer.Ordinal);

        public Task<IdempotencyRecord?> TryGetAsync(
            string key,
            CancellationToken cancellationToken = default
        )
        {
            _records.TryGetValue(key, out var record);
            return Task.FromResult<IdempotencyRecord?>(record);
        }

        public Task StoreAsync(
            string key,
            IdempotencyRecord record,
            TimeSpan ttl,
            CancellationToken cancellationToken = default
        )
        {
            _records[key] = record;
            return Task.CompletedTask;
        }
    }

    public class InMemoryCacheHelper : ICacheHelper
    {
        // No-op cache for integration tests - always returns cache miss
        // This ensures tests always get fresh data from the database

        public Task<CacheResult<T>> GetAsync<T>(string key) =>
            // Always return cache miss
            Task.FromResult(CacheResult<T>.Miss());

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) =>
            // Don't actually cache anything in tests
            Task.CompletedTask;

        public Task RemoveAsync(string key) => Task.CompletedTask;
    }

    public class MockPaymentService : IPaymentService
    {
        public Task<ErrorOr.ErrorOr<CustomerResource>> CreateCustomer(
            string email,
            string name,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult<ErrorOr.ErrorOr<CustomerResource>>(
                new CustomerResource("cus_mock_123", email, name)
            );

        public Task<ErrorOr<Success>> CreateRefundAsync(
            string chargeId,
            CancellationToken cancellationToken
        ) => Task.FromResult<ErrorOr<Success>>(Result.Success);

        public ErrorOr.ErrorOr<PaymentWebhookEvent> ParseWebhookEvent(
            string jsonPayload,
            string signatureHeader
        )
        {
            if (signatureHeader.Contains("invalid", StringComparison.OrdinalIgnoreCase))
            {
                return ErrorOr.Error.Validation(
                    code: "stripe.webhook_signature_invalid",
                    description: "Invalid signature"
                );
            }

            return new PaymentWebhookEvent(
                PaymentWebhookEventType.PaymentSucceeded,
                null,
                "ch_mock_123",
                "cus_mock_123",
                "pi_mock_123",
                null
            );
        }

        public Task<ErrorOr.ErrorOr<CreateSaleResponse>> CreateSaleAsync(CreateSaleRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return Task.FromResult<ErrorOr.ErrorOr<CreateSaleResponse>>(
                new CreateSaleResponse
                {
                    ResponseStatusCode = 200,
                    CustomerId = request.CustomerId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    PaymentIntentId = "pi_mock_123",
                    ObjectType = "payment_intent",
                    CaptureMethod = "automatic",
                    ChargeId = "ch_mock_123",
                    PaymentMethodId = request.PaymentMethodId,
                    PaymentMethodTypes = request.PaymentMethodTypes ?? Array.Empty<string>(),
                    Status = "succeeded",
                    Metadata = new Dictionary<string, string>(StringComparer.Ordinal),
                }
            );
        }
    }

    public class MockMediaUploader : IMediaUploader
    {
        public Task<ErrorOr.ErrorOr<PhotoUploadResult>> UploadPhotoAsync(
            IFormFile file,
            CancellationToken cancellationToken = default
        ) =>
            Task.FromResult<ErrorOr.ErrorOr<PhotoUploadResult>>(
                new PhotoUploadResult("https://mock.cloudinary.com/image.jpg", "mock_public_id")
            );

        public Task<ErrorOr.ErrorOr<Success>> DeletePhotoAsync(string publicId) =>
            Task.FromResult<ErrorOr.ErrorOr<Success>>(Result.Success);
    }

    public async ValueTask InitializeAsync()
    {
        await EnsureContainerStartedAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"FATAL INITIALIZATION ERROR: {ex.Message}", ex);
        }
    }

    public new async ValueTask DisposeAsync() => await ValueTask.CompletedTask;

    private string BuildConnectionString()
    {
        var connectionString = SharedDbContainer.GetConnectionString();
        return connectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase)
            ? System.Text.RegularExpressions.Regex.Replace(
                connectionString,
                "Database=[^;]*",
                $"Database={_databaseName}",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            )
            : $"{connectionString};Database={_databaseName}";
    }

    private static async Task EnsureContainerStartedAsync()
    {
        if (ContainerStarted)
        {
            return;
        }

        await ContainerLock.WaitAsync();
        try
        {
            if (!ContainerStarted)
            {
                await SharedDbContainer.StartAsync();
                ContainerStarted = true;
            }
        }
        finally
        {
            ContainerLock.Release();
        }
    }
}
