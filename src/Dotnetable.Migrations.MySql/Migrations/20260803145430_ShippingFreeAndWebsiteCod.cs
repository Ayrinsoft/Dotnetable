using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class ShippingFreeAndWebsiteCod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowCashOnDelivery",
                table: "Websites",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FreeShippingMinOrderAmount",
                table: "Websites",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FreeShippingMinOrderAmountUsd",
                table: "Websites",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FreeShippingMinOrderAmount",
                table: "ShippingMethods",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FreeShippingMinOrderAmountUsd",
                table: "ShippingMethods",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowCashOnDelivery",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "FreeShippingMinOrderAmount",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "FreeShippingMinOrderAmountUsd",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "FreeShippingMinOrderAmount",
                table: "ShippingMethods");

            migrationBuilder.DropColumn(
                name: "FreeShippingMinOrderAmountUsd",
                table: "ShippingMethods");
        }
    }
}
