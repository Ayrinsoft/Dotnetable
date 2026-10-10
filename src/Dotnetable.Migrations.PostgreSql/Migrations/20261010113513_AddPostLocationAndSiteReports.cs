using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddPostLocationAndSiteReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasLocation",
                table: "Posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PostLocations",
                columns: table => new
                {
                    PostID = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CityID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostLocations", x => x.PostID);
                    table.ForeignKey(
                        name: "FK_PostLocations_Cities",
                        column: x => x.CityID,
                        principalTable: "Cities",
                        principalColumn: "CityID");
                    table.ForeignKey(
                        name: "FK_PostLocations_Posts",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteReports",
                columns: table => new
                {
                    SiteReportID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "integer", nullable: true),
                    ReporterName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmailAddress = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: false),
                    Kind = table.Column<byte>(type: "smallint", nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MessageBody = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    PageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SenderIPAddress = table.Column<string>(type: "character varying(45)", unicode: false, maxLength: 45, nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    AdminNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LogTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteReports", x => x.SiteReportID);
                    table.ForeignKey(
                        name: "FK_SiteReports_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                    table.ForeignKey(
                        name: "FK_SiteReports_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateTable(
                name: "PostLocationTranslations",
                columns: table => new
                {
                    PostLocationTranslationID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostID = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "character(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostLocationTranslations", x => x.PostLocationTranslationID);
                    table.ForeignKey(
                        name: "FK_PostLocationTranslations_PostLocations",
                        column: x => x.PostID,
                        principalTable: "PostLocations",
                        principalColumn: "PostID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostLocations_CityID",
                table: "PostLocations",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_PostLocationTranslations_PostID_LanguageCode",
                table: "PostLocationTranslations",
                columns: new[] { "PostID", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteReports_WebsiteClientID",
                table: "SiteReports",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_SiteReports_WebsiteID",
                table: "SiteReports",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_SiteReports_WebsiteID_Status",
                table: "SiteReports",
                columns: new[] { "WebsiteID", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostLocationTranslations");

            migrationBuilder.DropTable(
                name: "SiteReports");

            migrationBuilder.DropTable(
                name: "PostLocations");

            migrationBuilder.DropColumn(
                name: "HasLocation",
                table: "Posts");
        }
    }
}
