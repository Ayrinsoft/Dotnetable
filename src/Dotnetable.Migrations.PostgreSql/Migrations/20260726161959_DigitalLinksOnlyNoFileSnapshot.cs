using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class DigitalLinksOnlyNoFileSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDigitalAssets_FileRecords",
                table: "OrderDigitalAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_DigitalFile",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_DigitalFileID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_OrderDigitalAssets_DigitalFileID",
                table: "OrderDigitalAssets");

            migrationBuilder.DropColumn(
                name: "DigitalFileID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DigitalFileID",
                table: "OrderDigitalAssets");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "OrderDigitalAssets");

            migrationBuilder.AddColumn<string>(
                name: "DigitalDownloadUrl",
                table: "Products",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DigitalDownloadUrl",
                table: "OrderDigitalAssets",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DigitalDownloadUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DigitalDownloadUrl",
                table: "OrderDigitalAssets");

            migrationBuilder.AddColumn<int>(
                name: "DigitalFileID",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DigitalFileID",
                table: "OrderDigitalAssets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "OrderDigitalAssets",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_DigitalFileID",
                table: "Products",
                column: "DigitalFileID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDigitalAssets_DigitalFileID",
                table: "OrderDigitalAssets",
                column: "DigitalFileID");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDigitalAssets_FileRecords",
                table: "OrderDigitalAssets",
                column: "DigitalFileID",
                principalTable: "FileRecords",
                principalColumn: "FileRecordID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_DigitalFile",
                table: "Products",
                column: "DigitalFileID",
                principalTable: "FileRecords",
                principalColumn: "FileRecordID");
        }
    }
}
