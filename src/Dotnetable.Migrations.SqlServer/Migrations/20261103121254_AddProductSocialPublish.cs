using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSocialPublish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductInstagramPublications",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    IgMediaId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Permalink = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductInstagramPublications", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK_ProductInstagramPublications_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                });

            migrationBuilder.CreateTable(
                name: "ProductTelegramPublications",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    MessageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTelegramPublications", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK_ProductTelegramPublications_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                });

            migrationBuilder.CreateTable(
                name: "ProductXPublications",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    TweetId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductXPublications", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK_ProductXPublications_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductInstagramPublications");

            migrationBuilder.DropTable(
                name: "ProductTelegramPublications");

            migrationBuilder.DropTable(
                name: "ProductXPublications");
        }
    }
}
