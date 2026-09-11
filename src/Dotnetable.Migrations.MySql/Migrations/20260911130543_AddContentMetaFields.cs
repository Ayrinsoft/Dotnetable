using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddContentMetaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "PostTranslations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaKeywords",
                table: "PostTranslations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "PostTranslations",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "Posts",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaKeywords",
                table: "Posts",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "Posts",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "PageTranslations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaKeywords",
                table: "PageTranslations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "PageTranslations",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "Pages",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaKeywords",
                table: "Pages",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "Pages",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "PostTranslations");

            migrationBuilder.DropColumn(
                name: "MetaKeywords",
                table: "PostTranslations");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "PostTranslations");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "MetaKeywords",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "PageTranslations");

            migrationBuilder.DropColumn(
                name: "MetaKeywords",
                table: "PageTranslations");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "PageTranslations");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "MetaKeywords",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "Pages");
        }
    }
}
