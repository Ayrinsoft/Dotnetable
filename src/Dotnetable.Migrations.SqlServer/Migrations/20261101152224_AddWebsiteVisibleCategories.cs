using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteVisibleCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LimitVisibleCategories",
                table: "Websites",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "When true, the storefront shows only WebsiteVisibleProductCategories. Ancestors stay in the menu.");

            migrationBuilder.CreateTable(
                name: "WebsiteVisibleProductCategories",
                columns: table => new
                {
                    WebsiteVisibleProductCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    ProductCategoryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteVisibleProductCategories", x => x.WebsiteVisibleProductCategoryID);
                    table.ForeignKey(
                        name: "FK_WebsiteVisibleProductCategories_ProductCategories",
                        column: x => x.ProductCategoryID,
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WebsiteVisibleProductCategories_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteVisibleProductCategories_ProductCategoryID",
                table: "WebsiteVisibleProductCategories",
                column: "ProductCategoryID");

            migrationBuilder.CreateIndex(
                name: "UQ_WebsiteVisibleProductCategories_Website_Category",
                table: "WebsiteVisibleProductCategories",
                columns: new[] { "WebsiteID", "ProductCategoryID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebsiteVisibleProductCategories");

            migrationBuilder.DropColumn(
                name: "LimitVisibleCategories",
                table: "Websites");
        }
    }
}
