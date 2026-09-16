using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddContentComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CommentsEnabled",
                table: "Pages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ContentComments",
                columns: table => new
                {
                    ContentCommentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    PostID = table.Column<int>(type: "int", nullable: true),
                    PageID = table.Column<int>(type: "int", nullable: true),
                    ParentCommentID = table.Column<int>(type: "int", nullable: true),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: true),
                    AuthorMemberID = table.Column<int>(type: "int", nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AuthorEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    ModeratedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    ModeratedByMemberID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentComments", x => x.ContentCommentID);
                    table.ForeignKey(
                        name: "FK_ContentComments_ContentComments",
                        column: x => x.ParentCommentID,
                        principalTable: "ContentComments",
                        principalColumn: "ContentCommentID");
                    table.ForeignKey(
                        name: "FK_ContentComments_Members_Author",
                        column: x => x.AuthorMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_ContentComments_Members_Moderator",
                        column: x => x.ModeratedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_ContentComments_Pages",
                        column: x => x.PageID,
                        principalTable: "Pages",
                        principalColumn: "PageID");
                    table.ForeignKey(
                        name: "FK_ContentComments_Posts",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID");
                    table.ForeignKey(
                        name: "FK_ContentComments_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                    table.ForeignKey(
                        name: "FK_ContentComments_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentComments_AuthorMemberID",
                table: "ContentComments",
                column: "AuthorMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_ContentComments_ModeratedByMemberID",
                table: "ContentComments",
                column: "ModeratedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_ContentComments_PageID",
                table: "ContentComments",
                column: "PageID");

            migrationBuilder.CreateIndex(
                name: "IX_ContentComments_ParentCommentID",
                table: "ContentComments",
                column: "ParentCommentID");

            migrationBuilder.CreateIndex(
                name: "IX_ContentComments_PostID",
                table: "ContentComments",
                column: "PostID");

            migrationBuilder.CreateIndex(
                name: "IX_ContentComments_WebsiteClientID",
                table: "ContentComments",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_ContentComments_WebsiteID_Status",
                table: "ContentComments",
                columns: new[] { "WebsiteID", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentComments");

            migrationBuilder.DropColumn(
                name: "CommentsEnabled",
                table: "Pages");
        }
    }
}
