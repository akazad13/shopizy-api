using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopizy.Domain.Brands;
using Shopizy.Domain.Brands.ValueObjects;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Products;
using Shopizy.Domain.Products.Entities;
using Shopizy.Domain.Products.ValueObjects;

namespace Shopizy.Infrastructure.Products.Persistence;

public sealed class ProductConfigurations : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        ConfigureProductsTable(builder);
        ConfigureProductImagesTable(builder);
        ConfigureProductVariantsTable(builder);
    }

    private static void ConfigureProductsTable(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products").HasKey(p => p.Id);

        builder
            .Property(p => p.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => ProductId.Create(value));

        builder.Property(p => p.Name).HasMaxLength(50).IsRequired();
        builder.Property(p => p.ShortDescription).HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Highlights).HasMaxLength(1000).IsRequired(false);
        builder.Property(p => p.SKU).HasMaxLength(50);
        builder.Property(p => p.StockQuantity);
        builder.Property(p => p.Discount).HasPrecision(18, 2).IsRequired(false);
#pragma warning disable CS8625
        builder
            .Property(p => p.BrandId)
            .HasConversion(
                id => (object?)id == null ? (Guid?)null : id.Value,
                value => value.HasValue ? BrandId.Create(value.Value) : null
            )
            .IsRequired(false);
#pragma warning restore CS8625
        builder.Property(p => p.Barcode).HasMaxLength(50).IsRequired(false);
        builder.Property(p => p.Colors).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Sizes).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Tags).HasMaxLength(200).IsRequired(false);
        builder.Property(p => p.IsActive).HasDefaultValue(true);
        builder.Property(p => p.CreatedOn).HasColumnType("smalldatetime");
        builder.Property(p => p.ModifiedOn).HasColumnType("smalldatetime").IsRequired(false);

        builder.OwnsOne(
            p => p.UnitPrice,
            pb =>
            {
                pb.Property(p => p.Amount).HasPrecision(18, 2);
                pb.Property(p => p.Currency).HasConversion<int>();
                pb.HasIndex(p => p.Amount);
            }
        );
        builder.OwnsOne(
            p => p.AverageRating,
            avrb =>
            {
                avrb.Property(avr => avr.Value).HasPrecision(18, 2);
            }
        );

        builder
            .Property(p => p.CategoryId)
            .HasConversion(id => id.Value, value => CategoryId.Create(value));

        builder
            .HasOne<Brand>()
            .WithMany()
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(p => p.ProductImages).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(p => p.ProductReviews).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(p => p.ProductVariants).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.BrandId);
        builder.HasIndex(p => p.StockQuantity);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.CreatedOn);
        builder.HasIndex(p => new { p.CategoryId, p.IsActive });

        builder.Property<byte[]>("RowVersion");
    }

    private static void ConfigureProductImagesTable(EntityTypeBuilder<Product> builder) =>
        builder.OwnsMany(
            p => p.ProductImages,
            pib =>
            {
                pib.ToTable("ProductImages");

                pib.WithOwner().HasForeignKey("ProductId");
                pib.HasKey("ProductId", nameof(ProductImage.Id));

                pib.Property(pi => pi.PublicId).HasMaxLength(100);
                pib.Property(pi => pi.Id)
                    .ValueGeneratedNever()
                    .HasConversion(id => id.Value, value => ProductImageId.Create(value));
                pib.Property(pi => pi.ImageUrl).IsRequired();
            }
        );

    private static void ConfigureProductVariantsTable(EntityTypeBuilder<Product> builder) =>
        builder.OwnsMany(
            p => p.ProductVariants,
            pvb =>
            {
                pvb.ToTable("ProductVariants");
                pvb.WithOwner().HasForeignKey("ProductId");
                pvb.HasKey("ProductId", nameof(ProductVariant.Id));
                pvb.Property(v => v.Id)
                    .ValueGeneratedNever()
                    .HasConversion(id => id.Value, v => ProductVariantId.Create(v));
                pvb.Property(v => v.Name).HasMaxLength(100);
                pvb.Property(v => v.SKU).HasMaxLength(50);
                pvb.Property(v => v.StockQuantity);
                pvb.Property(v => v.IsActive).HasDefaultValue(true);
                pvb.OwnsOne(
                    v => v.UnitPrice,
                    pb =>
                    {
                        pb.Property(p => p.Amount).HasPrecision(18, 2);
                        pb.Property(p => p.Currency).HasConversion<int>();
                    }
                );
            }
        );
}
