using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteContactInfoTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebsiteContactInfoTranslations",
                columns: table => new
                {
                    WebsiteContactInfoTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteContactInfoID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    GroupTitle = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    Title = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteContactInfoTranslations", x => x.WebsiteContactInfoTranslationID);
                    table.ForeignKey(
                        name: "FK_WebsiteContactInfoTranslations_WebsiteContactInfos",
                        column: x => x.WebsiteContactInfoID,
                        principalTable: "WebsiteContactInfos",
                        principalColumn: "WebsiteContactInfoID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteContactInfoTranslations_WebsiteContactInfoID_LanguageCode",
                table: "WebsiteContactInfoTranslations",
                columns: new[] { "WebsiteContactInfoID", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebsiteContactInfoTranslations");
        }
    }
}
