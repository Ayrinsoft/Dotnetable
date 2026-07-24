using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class ProductPriceHistoryAndWarranties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductVariantPriceHistories",
                columns: table => new
                {
                    ProductVariantPriceHistoryID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductVariantID = table.Column<int>(type: "int", nullable: false),
                    ReferencePriceUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CompareAtPriceUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(sysutcdatetime())")
                        .Annotation("Relational:DefaultConstraintName", "DF_ProductVariantPriceHistories_RecordedAt"),
                    ChangedByMemberId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariantPriceHistories", x => x.ProductVariantPriceHistoryID);
                    table.ForeignKey(
                        name: "FK_ProductVariantPriceHistories_Members",
                        column: x => x.ChangedByMemberId,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_ProductVariantPriceHistories_ProductVariants",
                        column: x => x.ProductVariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "ProductVariantID");
                });

            migrationBuilder.CreateTable(
                name: "Warranties",
                columns: table => new
                {
                    WarrantyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProviderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DurationMonths = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_Warranties_IsActive"),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                        .Annotation("Relational:DefaultConstraintName", "DF_Warranties_SortOrder")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warranties", x => x.WarrantyID);
                    table.ForeignKey(
                        name: "FK_Warranties_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateTable(
                name: "ProductWarranties",
                columns: table => new
                {
                    ProductWarrantyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    WarrantyID = table.Column<int>(type: "int", nullable: true),
                    CustomTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                        .Annotation("Relational:DefaultConstraintName", "DF_ProductWarranties_SortOrder"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_ProductWarranties_IsActive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductWarranties", x => x.ProductWarrantyID);
                    table.ForeignKey(
                        name: "FK_ProductWarranties_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                    table.ForeignKey(
                        name: "FK_ProductWarranties_Warranties",
                        column: x => x.WarrantyID,
                        principalTable: "Warranties",
                        principalColumn: "WarrantyID");
                });

            migrationBuilder.CreateTable(
                name: "WarrantyTranslations",
                columns: table => new
                {
                    WarrantyTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarrantyID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProviderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyTranslations", x => x.WarrantyTranslationID);
                    table.ForeignKey(
                        name: "FK_WarrantyTranslations_Warranties",
                        column: x => x.WarrantyID,
                        principalTable: "Warranties",
                        principalColumn: "WarrantyID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantPriceHistories_ChangedByMemberId",
                table: "ProductVariantPriceHistories",
                column: "ChangedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantPriceHistories_ProductVariantID_RecordedAt",
                table: "ProductVariantPriceHistories",
                columns: new[] { "ProductVariantID", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductWarranties_ProductID",
                table: "ProductWarranties",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductWarranties_WarrantyID",
                table: "ProductWarranties",
                column: "WarrantyID");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_WebsiteID",
                table: "Warranties",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyTranslations_WarrantyID",
                table: "WarrantyTranslations",
                column: "WarrantyID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductVariantPriceHistories");

            migrationBuilder.DropTable(
                name: "ProductWarranties");

            migrationBuilder.DropTable(
                name: "WarrantyTranslations");

            migrationBuilder.DropTable(
                name: "Warranties");
        }
    }
}
