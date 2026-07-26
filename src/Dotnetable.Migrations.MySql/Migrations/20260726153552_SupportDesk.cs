using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class SupportDesk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupportSessions",
                columns: table => new
                {
                    SupportSessionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: true),
                    SessionNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Channel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Category = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Subject = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    Tags = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    CellphoneSnapshot = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    CountryCodeSnapshot = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: true),
                    EmailSnapshot = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    CustomerNameSnapshot = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    RelatedOrderID = table.Column<int>(type: "int", nullable: true),
                    AssignedMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    FirstResponseAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastInteractionAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    SatisfactionRating = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    Archive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportSessions", x => x.SupportSessionID);
                    table.ForeignKey(
                        name: "FK_SupportSessions_AssignedMembers",
                        column: x => x.AssignedMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_SupportSessions_CreatedByMembers",
                        column: x => x.CreatedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_SupportSessions_Orders",
                        column: x => x.RelatedOrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK_SupportSessions_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                    table.ForeignKey(
                        name: "FK_SupportSessions_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SupportInteractions",
                columns: table => new
                {
                    SupportInteractionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SupportSessionID = table.Column<int>(type: "int", nullable: false),
                    InteractionType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Body = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    CallOutcome = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    FromStatus = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    ToStatus = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    RelatedOrderID = table.Column<int>(type: "int", nullable: true),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    IsInternal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportInteractions", x => x.SupportInteractionID);
                    table.ForeignKey(
                        name: "FK_SupportInteractions_Members",
                        column: x => x.CreatedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_SupportInteractions_Orders",
                        column: x => x.RelatedOrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK_SupportInteractions_SupportSessions",
                        column: x => x.SupportSessionID,
                        principalTable: "SupportSessions",
                        principalColumn: "SupportSessionID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SupportInteractions_CreatedAt",
                table: "SupportInteractions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SupportInteractions_CreatedByMemberID",
                table: "SupportInteractions",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportInteractions_RelatedOrderID",
                table: "SupportInteractions",
                column: "RelatedOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportInteractions_SupportSessionID",
                table: "SupportInteractions",
                column: "SupportSessionID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportSessions_AssignedMemberID",
                table: "SupportSessions",
                column: "AssignedMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportSessions_CellphoneSnapshot",
                table: "SupportSessions",
                column: "CellphoneSnapshot");

            migrationBuilder.CreateIndex(
                name: "IX_SupportSessions_CreatedAt",
                table: "SupportSessions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SupportSessions_CreatedByMemberID",
                table: "SupportSessions",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportSessions_RelatedOrderID",
                table: "SupportSessions",
                column: "RelatedOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportSessions_Status",
                table: "SupportSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SupportSessions_Website_SessionNumber",
                table: "SupportSessions",
                columns: new[] { "WebsiteID", "SessionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportSessions_WebsiteClientID",
                table: "SupportSessions",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportSessions_WebsiteID",
                table: "SupportSessions",
                column: "WebsiteID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportInteractions");

            migrationBuilder.DropTable(
                name: "SupportSessions");
        }
    }
}
