using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagingGatewaysAndLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageLogs",
                columns: table => new
                {
                    MessageLogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Recipient = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    RecipientName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    RecipientType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RecipientID = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    Body = table.Column<string>(type: "longtext", nullable: true),
                    IsBodyRedacted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Provider = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Source = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    Error = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    SentByMemberID = table.Column<int>(type: "int", nullable: true),
                    SentByName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageLogs", x => x.MessageLogID);
                    table.ForeignKey(
                        name: "FK_MessageLogs_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteWhatsAppSettings",
                columns: table => new
                {
                    WebsiteWhatsAppSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    SettingsJSON = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    SenderNumber = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteWhatsAppSettings", x => x.WebsiteWhatsAppSettingID);
                    table.ForeignKey(
                        name: "FK_WebsiteWhatsAppSettings_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MessageLogs_CreatedAt",
                table: "MessageLogs",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_MessageLogs_WebsiteID_CreatedAt",
                table: "MessageLogs",
                columns: new[] { "WebsiteID", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteWhatsAppSettings_WebsiteID",
                table: "WebsiteWhatsAppSettings",
                column: "WebsiteID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageLogs");

            migrationBuilder.DropTable(
                name: "WebsiteWhatsAppSettings");
        }
    }
}
