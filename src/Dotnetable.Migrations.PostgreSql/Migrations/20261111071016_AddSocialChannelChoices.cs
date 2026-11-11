using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialChannelChoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_WebsiteXAccounts_WebsiteID",
                table: "WebsiteXAccounts");

            migrationBuilder.DropIndex(
                name: "UQ_WebsiteTelegramAccounts_WebsiteID",
                table: "WebsiteTelegramAccounts");

            migrationBuilder.DropIndex(
                name: "UQ_WebsiteInstagramAccounts_WebsiteID",
                table: "WebsiteInstagramAccounts");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "WebsiteXAccounts",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "WebsiteXAccounts",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "WebsiteTelegramAccounts",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "WebsiteTelegramAccounts",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "WebsiteInstagramAccounts",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "WebsiteInstagramAccounts",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteXAccounts_WebsiteID",
                table: "WebsiteXAccounts",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteTelegramAccounts_WebsiteID",
                table: "WebsiteTelegramAccounts",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteInstagramAccounts_WebsiteID",
                table: "WebsiteInstagramAccounts",
                column: "WebsiteID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WebsiteXAccounts_WebsiteID",
                table: "WebsiteXAccounts");

            migrationBuilder.DropIndex(
                name: "IX_WebsiteTelegramAccounts_WebsiteID",
                table: "WebsiteTelegramAccounts");

            migrationBuilder.DropIndex(
                name: "IX_WebsiteInstagramAccounts_WebsiteID",
                table: "WebsiteInstagramAccounts");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "WebsiteXAccounts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "WebsiteXAccounts");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "WebsiteTelegramAccounts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "WebsiteTelegramAccounts");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "WebsiteInstagramAccounts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "WebsiteInstagramAccounts");

            migrationBuilder.CreateIndex(
                name: "UQ_WebsiteXAccounts_WebsiteID",
                table: "WebsiteXAccounts",
                column: "WebsiteID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_WebsiteTelegramAccounts_WebsiteID",
                table: "WebsiteTelegramAccounts",
                column: "WebsiteID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_WebsiteInstagramAccounts_WebsiteID",
                table: "WebsiteInstagramAccounts",
                column: "WebsiteID",
                unique: true);
        }
    }
}
