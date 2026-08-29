using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopizy.Domain.Users;
using Shopizy.Domain.Users.Entities;
using Shopizy.Domain.Users.ValueObjects;

namespace Shopizy.Infrastructure.Users.Persistence;

public sealed class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ConfigureUsersTable(builder);
        ConfigureUserPermissionsTable(builder);
        ConfigureProductReviewIdsTable(builder);
        ConfigureOrderIdsTable(builder);
        ConfigureUserAddressesTable(builder);
    }

    private static void ConfigureUsersTable(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users").HasKey(u => u.Id);
        builder.HasIndex(u => u.Email);

        builder
            .Property(u => u.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => UserId.Create(value));

        builder.Property(u => u.FirstName).HasMaxLength(50);
        builder.Property(u => u.LastName).HasMaxLength(50);

        builder.Property(u => u.Email).HasMaxLength(254).IsRequired(true);
        builder.Property(u => u.Phone).HasMaxLength(15).IsRequired(false);

        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(u => u.CreatedOn).HasColumnType("smalldatetime");
        builder.Property(u => u.ModifiedOn).HasColumnType("smalldatetime").IsRequired(false);

        builder.Property(c => c.ProfileImageUrl).IsRequired(false);
        builder.OwnsOne(
            c => c.Address,
            ab =>
            {
                ab.Property(ad => ad.Street).HasMaxLength(100).IsRequired();
                ab.Property(ad => ad.City).HasMaxLength(30).IsRequired();
                ab.Property(ad => ad.State).HasMaxLength(30).IsRequired();
                ab.Property(ad => ad.Country).HasMaxLength(30).IsRequired();
                ab.Property(ad => ad.ZipCode).HasMaxLength(10).IsRequired();
            }
        );

        builder.Property(u => u.CustomerId).HasMaxLength(256).IsRequired(false);

        builder.OwnsOne(
            u => u.Credentials,
            cb =>
            {
                cb.Property(c => c.Password).HasColumnName("Password").IsRequired(false);
                cb.Property(c => c.PasswordResetToken)
                    .HasColumnName("PasswordResetToken")
                    .HasMaxLength(256)
                    .IsRequired(false);
                cb.Property(c => c.PasswordResetTokenExpiry)
                    .HasColumnName("PasswordResetTokenExpiry")
                    .IsRequired(false);
                cb.Property(c => c.TwoFactorSecret)
                    .HasColumnName("TwoFactorSecret")
                    .HasMaxLength(64)
                    .IsRequired(false);
                cb.Property(c => c.IsTwoFactorEnabled)
                    .HasColumnName("IsTwoFactorEnabled")
                    .HasDefaultValue(false);
            }
        );

        builder.OwnsOne(
            u => u.NotificationPreferences,
            npb =>
            {
                npb.Property(np => np.EmailEnabled)
                    .HasColumnName("Notification_EmailEnabled")
                    .HasDefaultValue(true);
                npb.Property(np => np.OrderUpdates)
                    .HasColumnName("Notification_OrderUpdates")
                    .HasDefaultValue(true);
                npb.Property(np => np.Promotions)
                    .HasColumnName("Notification_Promotions")
                    .HasDefaultValue(true);
                npb.Property(np => np.PriceAlerts)
                    .HasColumnName("Notification_PriceAlerts")
                    .HasDefaultValue(true);
                npb.Property(np => np.RestockAlerts)
                    .HasColumnName("Notification_RestockAlerts")
                    .HasDefaultValue(true);
            }
        );

        builder.Navigation(p => p.OrderIds).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(p => p.ProductReviewIds).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<byte[]>("RowVersion");
    }

    private static void ConfigureUserPermissionsTable(EntityTypeBuilder<User> builder)
    {
        builder.OwnsMany(
            u => u.PermissionIds,
            permissionBuilder =>
            {
                permissionBuilder.ToTable("UserPermissionIds");

                permissionBuilder.WithOwner().HasForeignKey("UserId");

                permissionBuilder.HasKey("Id");

                permissionBuilder
                    .Property(d => d.Value)
                    .HasColumnName("PermissionId")
                    .ValueGeneratedNever();
            }
        );

        builder
            .Metadata.FindNavigation(nameof(User.PermissionIds))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureProductReviewIdsTable(EntityTypeBuilder<User> builder)
    {
        builder.OwnsMany(
            m => m.ProductReviewIds,
            reviewBuilder =>
            {
                reviewBuilder.ToTable("ProductReviewIds");

                reviewBuilder.WithOwner().HasForeignKey("UserId");

                reviewBuilder.HasKey("Id");

                reviewBuilder
                    .Property(r => r.Value)
                    .HasColumnName("ProductReviewId")
                    .ValueGeneratedNever();
            }
        );

        builder
            .Metadata.FindNavigation(nameof(User.ProductReviewIds))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureOrderIdsTable(EntityTypeBuilder<User> builder)
    {
        builder.OwnsMany(
            m => m.OrderIds,
            reviewBuilder =>
            {
                reviewBuilder.ToTable("OrderIds");

                reviewBuilder.WithOwner().HasForeignKey("UserId");

                reviewBuilder.HasKey("Id");

                reviewBuilder.Property(r => r.Value).HasColumnName("OrderId").ValueGeneratedNever();
            }
        );

        builder
            .Metadata.FindNavigation(nameof(User.OrderIds))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureUserAddressesTable(EntityTypeBuilder<User> builder)
    {
        builder.OwnsMany(
            u => u.Addresses,
            ab =>
            {
                ab.ToTable("UserAddresses");
                ab.WithOwner().HasForeignKey("UserId");
                ab.HasKey(nameof(UserAddress.Id), "UserId");
                ab.Property(a => a.Id)
                    .ValueGeneratedNever()
                    .HasConversion(id => id.Value, v => UserAddressId.Create(v));
                ab.Property(a => a.Street).HasMaxLength(100);
                ab.Property(a => a.City).HasMaxLength(50);
                ab.Property(a => a.State).HasMaxLength(50);
                ab.Property(a => a.Country).HasMaxLength(50);
                ab.Property(a => a.ZipCode).HasMaxLength(10);
                ab.Property(a => a.IsDefault).HasDefaultValue(false);
                ab.Property(a => a.CreatedOn).HasColumnType("smalldatetime");
                ab.Property(a => a.ModifiedOn).HasColumnType("smalldatetime").IsRequired(false);
            }
        );
        builder.Navigation(u => u.Addresses).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
