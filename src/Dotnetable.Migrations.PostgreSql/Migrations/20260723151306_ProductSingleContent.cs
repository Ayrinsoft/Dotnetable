using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class ProductSingleContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "ProductTranslations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Products" p
                SET "Content" = x."Merged"
                FROM (
                    SELECT s."ProductID",
                           string_agg(
                               CASE
                                   WHEN s."SectionType" = 1 AND f."CNDUrl" IS NOT NULL
                                       THEN '<p><img src="' || f."CNDUrl" || '" alt="" /></p>'
                                   WHEN s."HtmlContent" IS NOT NULL AND btrim(s."HtmlContent") <> ''
                                       THEN s."HtmlContent"
                                   ELSE ''
                               END,
                               '' ORDER BY s."SortOrder", s."ProductContentSectionID"
                           ) AS "Merged"
                    FROM "ProductContentSections" s
                    LEFT JOIN "FileRecords" f ON f."FileRecordID" = s."FileId"
                    WHERE s."IsActive" = TRUE
                    GROUP BY s."ProductID"
                ) x
                WHERE x."ProductID" = p."ProductID"
                  AND x."Merged" IS NOT NULL
                  AND btrim(x."Merged") <> '';
                """);

            migrationBuilder.DropTable(
                name: "ProductContentSectionTranslations");

            migrationBuilder.DropTable(
                name: "ProductContentSections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "ProductTranslations");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "ProductContentSections",
                columns: table => new
                {
                    ProductContentSectionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileId = table.Column<int>(type: "integer", nullable: true),
                    MediaSetID = table.Column<int>(type: "integer", nullable: true),
                    ProductID = table.Column<int>(type: "integer", nullable: false),
                    HtmlContent = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SectionType = table.Column<byte>(type: "smallint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductContentSections", x => x.ProductContentSectionID);
                    table.ForeignKey(
                        name: "FK_ProductContentSections_FileRecords",
                        column: x => x.FileId,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_ProductContentSections_MediaSets",
                        column: x => x.MediaSetID,
                        principalTable: "MediaSets",
                        principalColumn: "MediaSetID");
                    table.ForeignKey(
                        name: "FK_ProductContentSections_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                });

            migrationBuilder.CreateTable(
                name: "ProductContentSectionTranslations",
                columns: table => new
                {
                    ProductContentSectionTranslationID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductContentSectionID = table.Column<int>(type: "integer", nullable: false),
                    HtmlContent = table.Column<string>(type: "text", nullable: false),
                    LanguageCode = table.Column<string>(type: "character(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductContentSectionTranslations", x => x.ProductContentSectionTranslationID);
                    table.ForeignKey(
                        name: "FK_ProductContentSectionTranslations_ProductContentSections",
                        column: x => x.ProductContentSectionID,
                        principalTable: "ProductContentSections",
                        principalColumn: "ProductContentSectionID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductContentSections_FileId",
                table: "ProductContentSections",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductContentSections_MediaSetID",
                table: "ProductContentSections",
                column: "MediaSetID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductContentSections_ProductID",
                table: "ProductContentSections",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductContentSectionTranslations_ProductContentSectionID",
                table: "ProductContentSectionTranslations",
                column: "ProductContentSectionID");
        }
    }
}
