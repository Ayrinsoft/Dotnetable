using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class ShippingAndDigitalProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CodMinPrice",
                table: "ShippingMethods",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CodMinPriceUsd",
                table: "ShippingMethods",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LogoFileID",
                table: "ShippingMethods",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepaidMinPrice",
                table: "ShippingMethods",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepaidMinPriceUsd",
                table: "ShippingMethods",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsCod",
                table: "ShippingMethods",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsPrepaid",
                table: "ShippingMethods",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "DigitalDeliveryNote",
                table: "Products",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DigitalFileID",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DigitalServiceUrl",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "ProductType",
                table: "Products",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresShipping",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingMethods_LogoFileID",
                table: "ShippingMethods",
                column: "LogoFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DigitalFileID",
                table: "Products",
                column: "DigitalFileID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_DigitalFile",
                table: "Products",
                column: "DigitalFileID",
                principalTable: "FileRecords",
                principalColumn: "FileRecordID");

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingMethods_FileRecords",
                table: "ShippingMethods",
                column: "LogoFileID",
                principalTable: "FileRecords",
                principalColumn: "FileRecordID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_DigitalFile",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingMethods_FileRecords",
                table: "ShippingMethods");

            migrationBuilder.DropIndex(
                name: "IX_ShippingMethods_LogoFileID",
                table: "ShippingMethods");

            migrationBuilder.DropIndex(
                name: "IX_Products_DigitalFileID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CodMinPrice",
                table: "ShippingMethods");

            migrationBuilder.DropColumn(
                name: "CodMinPriceUsd",
                table: "ShippingMethods");

            migrationBuilder.DropColumn(
                name: "LogoFileID",
                table: "ShippingMethods");

            migrationBuilder.DropColumn(
                name: "PrepaidMinPrice",
                table: "ShippingMethods");

            migrationBuilder.DropColumn(
                name: "PrepaidMinPriceUsd",
                table: "ShippingMethods");

            migrationBuilder.DropColumn(
                name: "SupportsCod",
                table: "ShippingMethods");

            migrationBuilder.DropColumn(
                name: "SupportsPrepaid",
                table: "ShippingMethods");

            migrationBuilder.DropColumn(
                name: "DigitalDeliveryNote",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DigitalFileID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DigitalServiceUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RequiresShipping",
                table: "Products");
        }
    }
}
