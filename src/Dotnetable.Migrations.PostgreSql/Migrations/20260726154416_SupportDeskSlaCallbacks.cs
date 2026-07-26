using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class SupportDeskSlaCallbacks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CallbackAt",
                table: "SupportSessions",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallbackNote",
                table: "SupportSessions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstResponseDueAt",
                table: "SupportSessions",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolveDueAt",
                table: "SupportSessions",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallbackAt",
                table: "SupportSessions");

            migrationBuilder.DropColumn(
                name: "CallbackNote",
                table: "SupportSessions");

            migrationBuilder.DropColumn(
                name: "FirstResponseDueAt",
                table: "SupportSessions");

            migrationBuilder.DropColumn(
                name: "ResolveDueAt",
                table: "SupportSessions");
        }
    }
}
