using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Shopizy.Infrastructure.Common.Persistence;

#nullable disable

namespace Shopizy.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260823000000_AddNotificationPreferencesToUser")]
    public partial class AddNotificationPreferencesToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Notification_EmailEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "Notification_SmsEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "Notification_PushEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "Notification_OrderUpdates",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "Notification_Promotions",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "Notification_PriceAlerts",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "Notification_RestockAlerts",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Notification_EmailEnabled", table: "Users");
            migrationBuilder.DropColumn(name: "Notification_SmsEnabled", table: "Users");
            migrationBuilder.DropColumn(name: "Notification_PushEnabled", table: "Users");
            migrationBuilder.DropColumn(name: "Notification_OrderUpdates", table: "Users");
            migrationBuilder.DropColumn(name: "Notification_Promotions", table: "Users");
            migrationBuilder.DropColumn(name: "Notification_PriceAlerts", table: "Users");
            migrationBuilder.DropColumn(name: "Notification_RestockAlerts", table: "Users");
        }
    }
}
