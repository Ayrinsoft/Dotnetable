using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteBackground : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundColor",
                table: "Websites",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BackgroundImageFileID",
                table: "Websites",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DarkBackgroundColor",
                table: "Websites",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DarkBackgroundImageFileID",
                table: "Websites",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Websites_BackgroundImageFileID",
                table: "Websites",
                column: "BackgroundImageFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_DarkBackgroundImageFileID",
                table: "Websites",
                column: "DarkBackgroundImageFileID");

            migrationBuilder.AddForeignKey(
                name: "FK_Websites_FileRecords_Background",
                table: "Websites",
                column: "BackgroundImageFileID",
                principalTable: "FileRecords",
                principalColumn: "FileRecordID");

            migrationBuilder.AddForeignKey(
                name: "FK_Websites_FileRecords_DarkBackground",
                table: "Websites",
                column: "DarkBackgroundImageFileID",
                principalTable: "FileRecords",
                principalColumn: "FileRecordID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Websites_FileRecords_Background",
                table: "Websites");

            migrationBuilder.DropForeignKey(
                name: "FK_Websites_FileRecords_DarkBackground",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Websites_BackgroundImageFileID",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Websites_DarkBackgroundImageFileID",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "BackgroundColor",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "BackgroundImageFileID",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "DarkBackgroundColor",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "DarkBackgroundImageFileID",
                table: "Websites");
        }
    }
}
