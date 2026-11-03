using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddPostGalleryAndInstagram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostImages",
                columns: table => new
                {
                    PostImageID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostID = table.Column<int>(type: "integer", nullable: false),
                    FileRecordID = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostImages", x => x.PostImageID);
                    table.ForeignKey(
                        name: "FK_PostImages_FileRecords",
                        column: x => x.FileRecordID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_PostImages_Posts",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID");
                });

            migrationBuilder.CreateTable(
                name: "PostInstagramPublications",
                columns: table => new
                {
                    PostID = table.Column<int>(type: "integer", nullable: false),
                    IgMediaId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Permalink = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostInstagramPublications", x => x.PostID);
                    table.ForeignKey(
                        name: "FK_PostInstagramPublications_Posts",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID");
                });

            migrationBuilder.CreateTable(
                name: "WebsiteInstagramAccounts",
                columns: table => new
                {
                    WebsiteInstagramAccountID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    IgUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    PageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteInstagramAccounts", x => x.WebsiteInstagramAccountID);
                    table.ForeignKey(
                        name: "FK_WebsiteInstagramAccounts_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostImages_FileRecordID",
                table: "PostImages",
                column: "FileRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_PostImages_PostID",
                table: "PostImages",
                column: "PostID");

            migrationBuilder.CreateIndex(
                name: "UQ_PostImages_Post_File",
                table: "PostImages",
                columns: new[] { "PostID", "FileRecordID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_WebsiteInstagramAccounts_WebsiteID",
                table: "WebsiteInstagramAccounts",
                column: "WebsiteID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostImages");

            migrationBuilder.DropTable(
                name: "PostInstagramPublications");

            migrationBuilder.DropTable(
                name: "WebsiteInstagramAccounts");
        }
    }
}
