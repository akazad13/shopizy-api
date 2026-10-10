using Shopizy.Api;
using Shopizy.Api.Common.Middleware;
using Shopizy.Api.Common.Telemetry;
using Shopizy.Api.Endpoints;
using Shopizy.Application;
using Shopizy.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const long MaxRequestBytes = 10L * 1024 * 1024; // 10 MB

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = MaxRequestBytes;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxRequestBytes;
    options.ValueLengthLimit = (int)Math.Min(MaxRequestBytes, int.MaxValue);
});

builder
    .Services.AddPresentation()
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddTelemetry(builder.Configuration, builder.Environment)
    .AddEndpoints(typeof(Program).Assembly);

var app = builder.Build();

app.UseInfrastructure();
app.MapEndpoints();
app.MapHub<Shopizy.Infrastructure.Realtime.Hubs.OrderStatusHub>("/hubs/orders");
app.MapHub<Shopizy.Infrastructure.Realtime.Hubs.AdminDashboardHub>("/hubs/admin-dashboard");
app.MapHealthChecks("/healthz").DisableRateLimiting();

// Configure the HTTP request pipeline.
app.UseSwagger();

// Redirect root requests to /swagger/index.html so the UI loads
app.Use(
    async (context, next) =>
    {
        if (context.Request.Path == "" || context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger/index.html");
            return;
        }

        await next();
    }
);

app.UseSwaggerUI();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestBufferingMiddleware>();

app.UseExceptionHandler();

app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseCors("_myAllowSpecificOrigins");

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication().UseAuthorization().UseRateLimiter();

await app.RunAsync();
