using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
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
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            // Merge previous multi-section bodies into a single HTML field (text sections + image URLs).
            migrationBuilder.Sql("""
                UPDATE p
                SET Content = x.Merged
                FROM Products p
                INNER JOIN (
                    SELECT s.ProductID,
                           STRING_AGG(
                               CASE
                                   WHEN s.SectionType = 1 AND f.CNDUrl IS NOT NULL
                                       THEN CONCAT(N'<p><img src="', f.CNDUrl, N'" alt="" /></p>')
                                   WHEN s.HtmlContent IS NOT NULL AND LTRIM(RTRIM(s.HtmlContent)) <> N''
                                       THEN s.HtmlContent
                                   ELSE N''
                               END,
                               N''
                           ) WITHIN GROUP (ORDER BY s.SortOrder, s.ProductContentSectionID) AS Merged
                    FROM ProductContentSections s
                    LEFT JOIN FileRecords f ON f.FileRecordID = s.FileId
                    WHERE s.IsActive = 1
                    GROUP BY s.ProductID
                ) x ON x.ProductID = p.ProductID
                WHERE x.Merged IS NOT NULL AND LTRIM(RTRIM(x.Merged)) <> N'';
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<int>(type: "int", nullable: true),
                    MediaSetID = table.Column<int>(type: "int", nullable: true),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_ProductContentSections_IsActive"),
                    SectionType = table.Column<byte>(type: "tinyint", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "ProductContentSectionTranslations",
                columns: table => new
                {
                    ProductContentSectionTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductContentSectionID = table.Column<int>(type: "int", nullable: false),
                    HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
