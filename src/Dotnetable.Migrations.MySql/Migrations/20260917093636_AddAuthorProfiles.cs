using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthorProfiles",
                columns: table => new
                {
                    AuthorProfileID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MemberID = table.Column<int>(type: "int", nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Headline = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Bio = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    ShowBioOnPosts = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ResumeEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    About = table.Column<string>(type: "longtext", nullable: true),
                    Location = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    PublicEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true),
                    SocialLinksJson = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    Skills = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    PhotoFileID = table.Column<int>(type: "int", nullable: true),
                    ResumeFileID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorProfiles", x => x.AuthorProfileID);
                    table.ForeignKey(
                        name: "FK_AuthorProfiles_FileRecords_Photo",
                        column: x => x.PhotoFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_AuthorProfiles_FileRecords_Resume",
                        column: x => x.ResumeFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_AuthorProfiles_Members",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorProfiles_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AuthorProfileTranslations",
                columns: table => new
                {
                    AuthorProfileTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AuthorProfileID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Headline = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Bio = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    About = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorProfileTranslations", x => x.AuthorProfileTranslationID);
                    table.ForeignKey(
                        name: "FK_AuthorProfileTranslations_AuthorProfiles",
                        column: x => x.AuthorProfileID,
                        principalTable: "AuthorProfiles",
                        principalColumn: "AuthorProfileID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AuthorResumeItems",
                columns: table => new
                {
                    AuthorResumeItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AuthorProfileID = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Organization = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    Url = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsCurrent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShowInTimeline = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorResumeItems", x => x.AuthorResumeItemID);
                    table.ForeignKey(
                        name: "FK_AuthorResumeItems_AuthorProfiles",
                        column: x => x.AuthorProfileID,
                        principalTable: "AuthorProfiles",
                        principalColumn: "AuthorProfileID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AuthorResumeItemTranslations",
                columns: table => new
                {
                    AuthorResumeItemTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AuthorResumeItemID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Organization = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorResumeItemTranslations", x => x.AuthorResumeItemTranslationID);
                    table.ForeignKey(
                        name: "FK_AuthorResumeItemTranslations_AuthorResumeItems",
                        column: x => x.AuthorResumeItemID,
                        principalTable: "AuthorResumeItems",
                        principalColumn: "AuthorResumeItemID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorProfiles_MemberID",
                table: "AuthorProfiles",
                column: "MemberID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthorProfiles_PhotoFileID",
                table: "AuthorProfiles",
                column: "PhotoFileID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorProfiles_ResumeFileID",
                table: "AuthorProfiles",
                column: "ResumeFileID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorProfiles_WebsiteID_Slug",
                table: "AuthorProfiles",
                columns: new[] { "WebsiteID", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthorProfileTranslations_AuthorProfileID_LanguageCode",
                table: "AuthorProfileTranslations",
                columns: new[] { "AuthorProfileID", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthorResumeItems_AuthorProfileID",
                table: "AuthorResumeItems",
                column: "AuthorProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorResumeItemTranslations_AuthorResumeItemID_LanguageCode",
                table: "AuthorResumeItemTranslations",
                columns: new[] { "AuthorResumeItemID", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorProfileTranslations");

            migrationBuilder.DropTable(
                name: "AuthorResumeItemTranslations");

            migrationBuilder.DropTable(
                name: "AuthorResumeItems");

            migrationBuilder.DropTable(
                name: "AuthorProfiles");
        }
    }
}
