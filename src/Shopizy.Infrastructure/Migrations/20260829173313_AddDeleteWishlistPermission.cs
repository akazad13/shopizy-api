using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopizy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeleteWishlistPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name" },
                values: new object[]
                {
                    new Guid("11a0778b-51b4-4ca5-9ce8-7359deb36d33"),
                    "delete:wishlist",
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("11a0778b-51b4-4ca5-9ce8-7359deb36d33")
            );
        }
    }
}
