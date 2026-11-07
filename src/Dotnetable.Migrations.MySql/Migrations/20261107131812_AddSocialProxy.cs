using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialProxy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SocialProxyKey",
                table: "Websites",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialProxyUrl",
                table: "Websites",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true,
                comment: "Optional https origin of a Cloudflare Worker that relays Instagram, Telegram, and X.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SocialProxyKey",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "SocialProxyUrl",
                table: "Websites");
        }
    }
}
