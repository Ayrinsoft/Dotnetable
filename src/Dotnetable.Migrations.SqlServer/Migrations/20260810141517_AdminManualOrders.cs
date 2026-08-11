using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AdminManualOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReportOfflineOrdersToTax",
                table: "Websites",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupTotal",
                table: "Orders",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "ReportToTax",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte>(
                name: "SalesChannel",
                table: "Orders",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.AddColumn<decimal>(
                name: "CatalogUnitPrice",
                table: "OrderItems",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitMarkup",
                table: "OrderItems",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportOfflineOrdersToTax",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "MarkupTotal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReportToTax",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SalesChannel",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CatalogUnitPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UnitMarkup",
                table: "OrderItems");
        }
    }
}
