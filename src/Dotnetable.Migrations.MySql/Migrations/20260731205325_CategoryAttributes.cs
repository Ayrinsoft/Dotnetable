using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class CategoryAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductCategoryAttributes",
                columns: table => new
                {
                    ProductCategoryID = table.Column<int>(type: "int", nullable: false),
                    AttributeDefinitionID = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryAttributes", x => new { x.ProductCategoryID, x.AttributeDefinitionID });
                    table.ForeignKey(
                        name: "FK_ProductCategoryAttributes_AttributeDefinitions",
                        column: x => x.AttributeDefinitionID,
                        principalTable: "AttributeDefinitions",
                        principalColumn: "AttributeDefinitionID");
                    table.ForeignKey(
                        name: "FK_ProductCategoryAttributes_ProductCategories",
                        column: x => x.ProductCategoryID,
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryAttributes_AttributeDefinitionID",
                table: "ProductCategoryAttributes",
                column: "AttributeDefinitionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCategoryAttributes");
        }
    }
}
