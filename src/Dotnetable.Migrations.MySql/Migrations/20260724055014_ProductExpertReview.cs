using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class ProductExpertReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpertReview",
                table: "ProductTranslations",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpertReview",
                table: "Products",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpertReview",
                table: "ProductTranslations");

            migrationBuilder.DropColumn(
                name: "ExpertReview",
                table: "Products");
        }
    }
}
