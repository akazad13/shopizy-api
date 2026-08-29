using Microsoft.Extensions.DependencyInjection;
using Shopizy.Application.Common.Interfaces.Authentication;
using Shopizy.Infrastructure.Security.Hashing;

namespace Shopizy.Infrastructure.DependencyInjection;

public static class SecurityRegister
{
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddScoped<IPasswordManager, PasswordManager>();

        const string claimName = "permissions";
        const string adminRole = "Admin";

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                "Product.Create",
                policy => policy.RequireClaim(claimName, "create:product").RequireRole(adminRole)
            )
            .AddPolicy(
                "Product.Modify",
                policy => policy.RequireClaim(claimName, "modify:product").RequireRole(adminRole)
            )
            .AddPolicy(
                "Product.Delete",
                policy => policy.RequireClaim(claimName, "delete:product").RequireRole(adminRole)
            )
            .AddPolicy(
                "Category.Create",
                policy => policy.RequireClaim(claimName, "create:category").RequireRole(adminRole)
            )
            .AddPolicy(
                "Category.Modify",
                policy => policy.RequireClaim(claimName, "modify:category").RequireRole(adminRole)
            )
            .AddPolicy(
                "Category.Delete",
                policy => policy.RequireClaim(claimName, "delete:category").RequireRole(adminRole)
            )
            .AddPolicy("Order.Create", policy => policy.RequireClaim(claimName, "create:order"))
            .AddPolicy("Order.Get", policy => policy.RequireClaim(claimName, "get:order"))
            .AddPolicy("Order.Read", policy => policy.RequireClaim(claimName, "get:order"))
            .AddPolicy("Order.Modify", policy => policy.RequireClaim(claimName, "modify:order"))
            .AddPolicy("Order.Delete", policy => policy.RequireClaim(claimName, "delete:order"))
            .AddPolicy("Cart.Create", policy => policy.RequireClaim(claimName, "create:cart"))
            .AddPolicy("Cart.Get", policy => policy.RequireClaim(claimName, "get:cart"))
            .AddPolicy("Cart.Modify", policy => policy.RequireClaim(claimName, "modify:cart"))
            .AddPolicy("Cart.Delete", policy => policy.RequireClaim(claimName, "delete:cart"))
            .AddPolicy(
                "Wishlist.Create",
                policy => policy.RequireClaim(claimName, "create:wishlist")
            )
            .AddPolicy("Wishlist.Get", policy => policy.RequireClaim(claimName, "get:wishlist"))
            .AddPolicy(
                "Wishlist.Modify",
                policy => policy.RequireClaim(claimName, "modify:wishlist")
            )
            .AddPolicy(
                "Wishlist.Delete",
                policy => policy.RequireClaim(claimName, "delete:wishlist")
            )
            .AddPolicy("User.Get", policy => policy.RequireClaim(claimName, "get:user"))
            .AddPolicy("User.Modify", policy => policy.RequireClaim(claimName, "modify:user"))
            .AddPolicy("Admin", policy => policy.RequireRole(adminRole))
            .AddPolicy("Admin.View", policy => policy.RequireRole(adminRole))
            .AddPolicy("Admin.Reports", policy => policy.RequireRole(adminRole))
            .AddPolicy("Admin.ViewOrder", policy => policy.RequireRole(adminRole))
            .AddPolicy("Admin.ViewOrders", policy => policy.RequireRole(adminRole))
            .AddPolicy("Admin.ViewUsers", policy => policy.RequireRole(adminRole))
            .AddPolicy("Admin.UpdateOrderStatus", policy => policy.RequireRole(adminRole))
            .AddPolicy("Admin.UpdateUserRole", policy => policy.RequireRole(adminRole))
            .AddPolicy("Order.Manage", policy => policy.RequireRole(adminRole))
            .AddPolicy(
                "ProductReview.Create",
                policy => policy.RequireClaim(claimName, "create:review")
            )
            .AddPolicy("ProductReview.Get", policy => policy.RequireClaim(claimName, "get:review"))
            .AddPolicy("Admin.DeleteProductReview", policy => policy.RequireRole(adminRole))
            .AddPolicy("Admin.PromoCode.Create", policy => policy.RequireRole(adminRole))
            .AddPolicy("Admin.PromoCode.Get", policy => policy.RequireRole(adminRole))
            .AddPolicy("Admin.PromoCode.Modify", policy => policy.RequireRole(adminRole))
            .AddPolicy("Admin.PromoCode.Delete", policy => policy.RequireRole(adminRole))
            .AddPolicy("Loyalty.Get", policy => policy.RequireClaim(claimName, "get:user"))
            .AddPolicy("Loyalty.Earn", policy => policy.RequireRole(adminRole))
            .AddPolicy("Loyalty.Redeem", policy => policy.RequireClaim(claimName, "modify:user"))
            .AddPolicy("GiftCard.Create", policy => policy.RequireRole(adminRole))
            .AddPolicy("GiftCard.Get", policy => policy.RequireRole(adminRole))
            .AddPolicy("GiftCard.Validate", policy => policy.RequireAuthenticatedUser())
            .AddPolicy("Question.Ask", policy => policy.RequireClaim(claimName, "get:product"))
            .AddPolicy("Question.Answer", policy => policy.RequireRole(adminRole))
            .AddPolicy("AuditLog.Get", policy => policy.RequireRole(adminRole))
            .SetFallbackPolicy(
                new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build()
            );

        return services;
    }
}
