using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialPublicLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SocialCategoryPath",
                table: "Websites",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true,
                comment: "Category path ending in {slug}. Empty keeps Blog/Category/{slug}.");

            migrationBuilder.AddColumn<string>(
                name: "SocialLinkBaseUrl",
                table: "Websites",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                comment: "Public origin appended to social captions. Empty uses WebsiteAddress.");

            migrationBuilder.AddColumn<string>(
                name: "SocialPagePath",
                table: "Websites",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true,
                comment: "Page path ending in {slug}. Empty keeps page/{slug}.");

            migrationBuilder.AddColumn<string>(
                name: "SocialPostPath",
                table: "Websites",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true,
                comment: "Post path ending in {slug}. Empty keeps Blog/Post/{slug}.");

            migrationBuilder.AddColumn<string>(
                name: "SocialProductPath",
                table: "Websites",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true,
                comment: "Product path ending in {slug}. Empty keeps Shop/Product/{slug}.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SocialCategoryPath",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "SocialLinkBaseUrl",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "SocialPagePath",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "SocialPostPath",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "SocialProductPath",
                table: "Websites");
        }
    }
}
