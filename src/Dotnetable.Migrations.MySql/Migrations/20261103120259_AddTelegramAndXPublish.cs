using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramAndXPublish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostTelegramPublications",
                columns: table => new
                {
                    PostID = table.Column<int>(type: "int", nullable: false),
                    MessageId = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostTelegramPublications", x => x.PostID);
                    table.ForeignKey(
                        name: "FK_PostTelegramPublications_Posts",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PostXPublications",
                columns: table => new
                {
                    PostID = table.Column<int>(type: "int", nullable: false),
                    TweetId = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostXPublications", x => x.PostID);
                    table.ForeignKey(
                        name: "FK_PostXPublications_Posts",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteTelegramAccounts",
                columns: table => new
                {
                    WebsiteTelegramAccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    BotToken = table.Column<string>(type: "longtext", nullable: false),
                    ChatId = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteTelegramAccounts", x => x.WebsiteTelegramAccountID);
                    table.ForeignKey(
                        name: "FK_WebsiteTelegramAccounts_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteXAccounts",
                columns: table => new
                {
                    WebsiteXAccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    ApiKey = table.Column<string>(type: "longtext", nullable: false),
                    ApiSecret = table.Column<string>(type: "longtext", nullable: false),
                    AccessToken = table.Column<string>(type: "longtext", nullable: false),
                    AccessTokenSecret = table.Column<string>(type: "longtext", nullable: false),
                    Username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteXAccounts", x => x.WebsiteXAccountID);
                    table.ForeignKey(
                        name: "FK_WebsiteXAccounts_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "UQ_WebsiteTelegramAccounts_WebsiteID",
                table: "WebsiteTelegramAccounts",
                column: "WebsiteID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_WebsiteXAccounts_WebsiteID",
                table: "WebsiteXAccounts",
                column: "WebsiteID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostTelegramPublications");

            migrationBuilder.DropTable(
                name: "PostXPublications");

            migrationBuilder.DropTable(
                name: "WebsiteTelegramAccounts");

            migrationBuilder.DropTable(
                name: "WebsiteXAccounts");
        }
    }
}
