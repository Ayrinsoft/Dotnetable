using System;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260923133854_AddFileMediaBackfill")]
    public partial class AddFileMediaBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "CacheControlAttempts",
                table: "FileRecords",
                type: "tinyint",
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
                type: "tinyint",
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
