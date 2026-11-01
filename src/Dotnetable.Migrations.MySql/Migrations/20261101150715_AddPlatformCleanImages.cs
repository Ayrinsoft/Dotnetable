using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformCleanImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RawCdnUrl",
                table: "FileRecords",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawStoragePath",
                table: "FileRecords",
                type: "varchar(350)",
                maxLength: 350,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawThumbnailCDN",
                table: "FileRecords",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawThumbnailStorage",
                table: "FileRecords",
                type: "varchar(350)",
                maxLength: 350,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RawCdnUrl",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "RawStoragePath",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "RawThumbnailCDN",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "RawThumbnailStorage",
                table: "FileRecords");
        }
    }
}
