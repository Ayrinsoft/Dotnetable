using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public class OrderFulfillmentShipping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "PreparationStatus",
                table: "Orders",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "ShippingStatus",
                table: "Orders",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "ShippingTrackingCode",
                table: "Orders",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreparationStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingTrackingCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippedAt",
                table: "Orders");
        }
    }
}
