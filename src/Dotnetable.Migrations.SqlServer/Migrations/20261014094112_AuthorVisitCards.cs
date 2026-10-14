using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AuthorVisitCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthorVisitCards",
                columns: table => new
                {
                    AuthorVisitCardID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberID = table.Column<int>(type: "int", nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Slug = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    BrandName = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Slogan = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Mobile = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    WhatsApp = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    LogoFileID = table.Column<int>(type: "int", nullable: true),
                    CoverFileID = table.Column<int>(type: "int", nullable: true),
                    CardColor = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    PageColor = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Field1Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Field1Value = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Field2Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Field2Value = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Field3Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Field3Value = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Field4Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Field4Value = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Field5Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Field5Value = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsPortrait = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CardLayout = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    ThemeMode = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TranslationsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SocialLinksJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneBookEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Bank1HolderName = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    Bank1Details = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Bank1Sheba = table.Column<string>(type: "varchar(26)", unicode: false, maxLength: 26, nullable: true),
                    Bank1CardNumber = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    Bank2HolderName = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    Bank2Details = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Bank2Sheba = table.Column<string>(type: "varchar(26)", unicode: false, maxLength: 26, nullable: true),
                    Bank2CardNumber = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorVisitCards", x => x.AuthorVisitCardID);
                    table.ForeignKey(
                        name: "FK_AuthorVisitCards_FileRecords_Cover",
                        column: x => x.CoverFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_AuthorVisitCards_FileRecords_Logo",
                        column: x => x.LogoFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_AuthorVisitCards_Members",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorVisitCards_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateTable(
                name: "AuthorVisitCardContacts",
                columns: table => new
                {
                    ContactID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorVisitCardID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    Mobile = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LogTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VisitorKey = table.Column<string>(type: "char(32)", unicode: false, fixedLength: true, maxLength: 32, nullable: true),
                    SavedContact = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorVisitCardContacts", x => x.ContactID);
                    table.ForeignKey(
                        name: "FK_AuthorVisitCardContacts_AuthorVisitCards",
                        column: x => x.AuthorVisitCardID,
                        principalTable: "AuthorVisitCards",
                        principalColumn: "AuthorVisitCardID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorVisitCardContacts_AuthorVisitCardID",
                table: "AuthorVisitCardContacts",
                column: "AuthorVisitCardID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorVisitCards_CoverFileID",
                table: "AuthorVisitCards",
                column: "CoverFileID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorVisitCards_LogoFileID",
                table: "AuthorVisitCards",
                column: "LogoFileID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorVisitCards_MemberID",
                table: "AuthorVisitCards",
                column: "MemberID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthorVisitCards_WebsiteID_Slug",
                table: "AuthorVisitCards",
                columns: new[] { "WebsiteID", "Slug" },
                unique: true,
                filter: "[Slug] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorVisitCardContacts");

            migrationBuilder.DropTable(
                name: "AuthorVisitCards");
        }
    }
}
