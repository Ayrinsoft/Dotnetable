using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Category",
                table: "Role",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "WebsiteID",
                table: "EmailSetting",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WebsiteID",
                table: "ContactUsMessage",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailSetting_WebsiteID",
                table: "EmailSetting",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_ContactUsMessage_WebsiteID",
                table: "ContactUsMessage",
                column: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactUsMessage_Website",
                table: "ContactUsMessage",
                column: "WebsiteID",
                principalTable: "Website",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSetting_Website",
                table: "EmailSetting",
                column: "WebsiteID",
                principalTable: "Website",
                principalColumn: "WebsiteID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactUsMessage_Website",
                table: "ContactUsMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSetting_Website",
                table: "EmailSetting");

            migrationBuilder.DropIndex(
                name: "IX_EmailSetting_WebsiteID",
                table: "EmailSetting");

            migrationBuilder.DropIndex(
                name: "IX_ContactUsMessage_WebsiteID",
                table: "ContactUsMessage");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "WebsiteID",
                table: "EmailSetting");

            migrationBuilder.DropColumn(
                name: "WebsiteID",
                table: "ContactUsMessage");
        }
    }
}
