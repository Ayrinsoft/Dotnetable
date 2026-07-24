using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
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
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpertReview",
                table: "Products",
                type: "text",
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
