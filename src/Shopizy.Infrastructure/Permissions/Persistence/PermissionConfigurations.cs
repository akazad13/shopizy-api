using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopizy.Domain.Permissions;
using Shopizy.Domain.Permissions.ValueObjects;

namespace Shopizy.Infrastructure.Permissions.Persistence;

public sealed class PermissionConfigurations : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder) => ConfigureUsersTable(builder);

    private static void ConfigureUsersTable(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions").HasKey(p => p.Id);

        builder
            .Property(p => p.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => PermissionId.Create(value));

        builder.Property(u => u.Name).HasMaxLength(50);

        builder.HasData(
            CreatePermission("249E733D-5BDC-49C3-91CA-06AE25A9C897", "create:cart"),
            CreatePermission("4B88CB16-0228-4669-BA7F-B75F42A3B7AF", "get:cart"),
            CreatePermission("20082930-3857-4B34-80D0-E256B9B585D8", "modify:cart"),
            CreatePermission("D6C2E3C6-314B-4F2E-A407-34139B145771", "delete:cart"),
            CreatePermission("F49BBC15-AA8B-4752-AF66-E3E00AFC173D", "create:category"),
            CreatePermission("5E2A486B-D9A0-4F83-8FF2-C56EF97CE485", "get:category"),
            CreatePermission("626DA392-0BBF-4C3F-8909-A8FC18F4DC43", "modify:category"),
            CreatePermission("6811001E-28AE-4BB1-BBB8-99BE33A21302", "delete:category"),
            CreatePermission("2A19090A-B3F3-4B30-9CED-934EE0503D26", "create:order"),
            CreatePermission("9601BA5E-EB54-4487-BFE0-563462D3CC25", "get:order"),
            CreatePermission("ACD9D507-AC45-4CD2-B0F4-91126C71319A", "modify:order"),
            CreatePermission("DD25381D-063C-4A3A-9539-DEEC640919A4", "delete:order"),
            CreatePermission("1DD03229-B8DA-4926-8E4A-12B27A0FF5E7", "create:product"),
            CreatePermission("0C65A58A-D472-4D5D-848E-EAC46F988F5D", "get:product"),
            CreatePermission("43B3188D-6E85-479A-9FD7-0186FCA97F52", "modify:product"),
            CreatePermission("1679BA61-9B46-457E-9974-F02300B9A1D5", "delete:product"),
            CreatePermission("0529A2F2-7507-4FA5-9DAF-68829F9D7FC4", "create:user"),
            CreatePermission("0374E597-604E-4146-8F40-8C994D26C290", "get:user"),
            CreatePermission("C920A577-1669-4167-B056-5E0A03329C55", "modify:user"),
            CreatePermission("80366E1A-634D-4579-9245-164166E1146B", "delete:user"),
            CreatePermission("9b259d3d-b634-4232-9deb-e5fdb20d7a64", "get:wishlist"),
            CreatePermission("759b8d6d-ffda-4c99-bf29-ed335c029a5c", "modify:wishlist"),
            CreatePermission("d99cab25-5af2-4b9c-9fad-385e4715d7f2", "create:wishlist"),
            CreatePermission("11a0778b-51b4-4ca5-9ce8-7359deb36d33", "delete:wishlist"),
            CreatePermission("b25fd8ef-723d-47d1-8b31-648a69733975", "get:dashboard"),
            CreatePermission("c3a1e8f2-4d7b-4e9a-b123-8f5d2e6a1c34", "create:review"),
            CreatePermission("a7b2c9d4-5e3f-4a1b-9c8d-2f6e0b4a7d91", "get:review")
        );
    }

    private static Permission CreatePermission(string id, string name)
    {
        var constructor = typeof(Permission).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new[] { typeof(PermissionId), typeof(string) },
            null
        );

        return (Permission)
            constructor!.Invoke(new object[] { PermissionId.Create(new Guid(id)), name });
    }
}
