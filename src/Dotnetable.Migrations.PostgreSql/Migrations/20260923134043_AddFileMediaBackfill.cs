using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddFileMediaBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "CacheControlAttempts",
                table: "FileRecords",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CacheControlSetAt",
                table: "FileRecords",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "ThumbnailAttempts",
                table: "FileRecords",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThumbnailCheckedAt",
                table: "FileRecords",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CacheControlAttempts",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "CacheControlSetAt",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "ThumbnailAttempts",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "ThumbnailCheckedAt",
                table: "FileRecords");
        }
    }
}
