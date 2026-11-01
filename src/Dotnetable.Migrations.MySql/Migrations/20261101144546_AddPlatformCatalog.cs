using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPlatform",
                table: "Websites",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                comment: "Website 1 only. When true, other websites use website 1's product catalog and cannot add products or change product categories.");

            migrationBuilder.AddColumn<int>(
                name: "GoogleProductCategoryId",
                table: "ProductCategories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CatalogRequests",
                columns: table => new
                {
                    CatalogRequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    RequestedByMemberID = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    ProposedCategoryName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ParentCategoryID = table.Column<int>(type: "int", nullable: true),
                    ProductTitle = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Details = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    AdminNote = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ReviewedByMemberID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogRequests", x => x.CatalogRequestID);
                    table.ForeignKey(
                        name: "FK_CatalogRequests_Members",
                        column: x => x.RequestedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_CatalogRequests_ProductCategories",
                        column: x => x.ParentCategoryID,
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryID");
                    table.ForeignKey(
                        name: "FK_CatalogRequests_ReviewedBy",
                        column: x => x.ReviewedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_CatalogRequests_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GoogleProductTaxonomyEntries",
                columns: table => new
                {
                    GoogleProductTaxonomyEntryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    GoogleId = table.Column<int>(type: "int", nullable: false),
                    ParentGoogleId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    FullPath = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    Depth = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    LastSeenUtc = table.Column<DateTime>(type: "datetime", nullable: false),
                    AbsentFromLatest = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleProductTaxonomyEntries", x => x.GoogleProductTaxonomyEntryID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "UQ_ProductCategories_GoogleProductCategoryId",
                table: "ProductCategories",
                column: "GoogleProductCategoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogRequests_ParentCategoryID",
                table: "CatalogRequests",
                column: "ParentCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogRequests_RequestedByMemberID",
                table: "CatalogRequests",
                column: "RequestedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogRequests_ReviewedByMemberID",
                table: "CatalogRequests",
                column: "ReviewedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogRequests_WebsiteID",
                table: "CatalogRequests",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogRequests_WebsiteID_Status",
                table: "CatalogRequests",
                columns: new[] { "WebsiteID", "Status" });

            migrationBuilder.CreateIndex(
                name: "UQ_GoogleProductTaxonomyEntries_GoogleId",
                table: "GoogleProductTaxonomyEntries",
                column: "GoogleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogRequests");

            migrationBuilder.DropTable(
                name: "GoogleProductTaxonomyEntries");

            migrationBuilder.DropIndex(
                name: "UQ_ProductCategories_GoogleProductCategoryId",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "IsPlatform",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "GoogleProductCategoryId",
                table: "ProductCategories");
        }
    }
}
