using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddFileManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FileAlbumID",
                table: "FileRecord",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FileAlbum",
                columns: table => new
                {
                    FileAlbumID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAlbum", x => x.FileAlbumID);
                    table.ForeignKey(
                        name: "FK_FileAlbum_Website",
                        column: x => x.WebsiteID,
                        principalTable: "Website",
                        principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateTable(
                name: "FileTag",
                columns: table => new
                {
                    FileTagID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTag", x => x.FileTagID);
                    table.ForeignKey(
                        name: "FK_FileTag_Website",
                        column: x => x.WebsiteID,
                        principalTable: "Website",
                        principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateTable(
                name: "FileRecordTag",
                columns: table => new
                {
                    FileRecordTagID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileRecordID = table.Column<int>(type: "int", nullable: false),
                    FileTagID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileRecordTag", x => x.FileRecordTagID);
                    table.ForeignKey(
                        name: "FK_FileRecordTag_FileRecord",
                        column: x => x.FileRecordID,
                        principalTable: "FileRecord",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_FileRecordTag_FileTag",
                        column: x => x.FileTagID,
                        principalTable: "FileTag",
                        principalColumn: "FileTagID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileRecord_FileAlbumID",
                table: "FileRecord",
                column: "FileAlbumID");

            migrationBuilder.CreateIndex(
                name: "IX_FileAlbum_WebsiteID",
                table: "FileAlbum",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecordTag_FileRecordID",
                table: "FileRecordTag",
                column: "FileRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecordTag_FileTagID",
                table: "FileRecordTag",
                column: "FileTagID");

            migrationBuilder.CreateIndex(
                name: "IX_FileTag_WebsiteID",
                table: "FileTag",
                column: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_FileRecord_FileAlbum",
                table: "FileRecord",
                column: "FileAlbumID",
                principalTable: "FileAlbum",
                principalColumn: "FileAlbumID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileRecord_FileAlbum",
                table: "FileRecord");

            migrationBuilder.DropTable(
                name: "FileAlbum");

            migrationBuilder.DropTable(
                name: "FileRecordTag");

            migrationBuilder.DropTable(
                name: "FileTag");

            migrationBuilder.DropIndex(
                name: "IX_FileRecord_FileAlbumID",
                table: "FileRecord");

            migrationBuilder.DropColumn(
                name: "FileAlbumID",
                table: "FileRecord");
        }
    }
}
