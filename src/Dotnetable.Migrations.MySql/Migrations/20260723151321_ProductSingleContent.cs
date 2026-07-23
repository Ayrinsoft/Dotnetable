using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
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
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Products",
                type: "longtext",
                nullable: true);

            // MySQL 8+: GROUP_CONCAT ordered merge of section HTML / images.
            migrationBuilder.Sql("""
                UPDATE Products p
                INNER JOIN (
                    SELECT s.ProductID,
                           GROUP_CONCAT(
                               CASE
                                   WHEN s.SectionType = 1 AND f.CNDUrl IS NOT NULL
                                       THEN CONCAT('<p><img src="', f.CNDUrl, '" alt="" /></p>')
                                   WHEN s.HtmlContent IS NOT NULL AND TRIM(s.HtmlContent) <> ''
                                       THEN s.HtmlContent
                                   ELSE ''
                               END
                               ORDER BY s.SortOrder, s.ProductContentSectionID
                               SEPARATOR ''
                           ) AS Merged
                    FROM ProductContentSections s
                    LEFT JOIN FileRecords f ON f.FileRecordID = s.FileId
                    WHERE s.IsActive = 1
                    GROUP BY s.ProductID
                ) x ON x.ProductID = p.ProductID
                SET p.Content = x.Merged
                WHERE x.Merged IS NOT NULL AND TRIM(x.Merged) <> '';
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
                    ProductContentSectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FileId = table.Column<int>(type: "int", nullable: true),
                    MediaSetID = table.Column<int>(type: "int", nullable: true),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    HtmlContent = table.Column<string>(type: "longtext", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    SectionType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
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
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductContentSectionTranslations",
                columns: table => new
                {
                    ProductContentSectionTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductContentSectionID = table.Column<int>(type: "int", nullable: false),
                    HtmlContent = table.Column<string>(type: "longtext", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductContentSectionTranslations", x => x.ProductContentSectionTranslationID);
                    table.ForeignKey(
                        name: "FK_ProductContentSectionTranslations_ProductContentSections",
                        column: x => x.ProductContentSectionID,
                        principalTable: "ProductContentSections",
                        principalColumn: "ProductContentSectionID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

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
