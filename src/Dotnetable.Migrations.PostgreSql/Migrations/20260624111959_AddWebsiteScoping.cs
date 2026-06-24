using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteScoping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WebsiteID",
                table: "Policy",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WebsiteID",
                table: "LoginTry",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Policy_WebsiteID",
                table: "Policy",
                column: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_Policy_Website",
                table: "Policy",
                column: "WebsiteID",
                principalTable: "Website",
                principalColumn: "WebsiteID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policy_Website",
                table: "Policy");

            migrationBuilder.DropIndex(
                name: "IX_Policy_WebsiteID",
                table: "Policy");

            migrationBuilder.DropColumn(
                name: "WebsiteID",
                table: "Policy");

            migrationBuilder.DropColumn(
                name: "WebsiteID",
                table: "LoginTry");
        }
    }
}
