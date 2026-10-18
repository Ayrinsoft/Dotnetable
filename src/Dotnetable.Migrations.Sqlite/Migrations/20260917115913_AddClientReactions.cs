using System;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260917115913_AddClientReactions")]
    public partial class AddClientReactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FavoriteCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "Pages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ClientReactions",
                columns: table => new
                {
                    ClientReactionID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false),
                    TargetType = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetID = table.Column<int>(type: "int", nullable: false),
                    ReactionType = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientReactions", x => x.ClientReactionID);
                    table.ForeignKey(
                        name: "FK_ClientReactions_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientReactions_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientReactions_Client_Target_Reaction",
                table: "ClientReactions",
                columns: new[] { "WebsiteClientID", "TargetType", "TargetID", "ReactionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientReactions_WebsiteID_Target",
                table: "ClientReactions",
                columns: new[] { "WebsiteID", "TargetType", "TargetID", "ReactionType" });

            // Backfill: every product already on a wishlist becomes that customer's favorite
            // (TargetType 1 = Product, ReactionType 1 = Like), counted once per customer.
            migrationBuilder.Sql(@"
INSERT INTO ""ClientReactions"" (""WebsiteID"", ""WebsiteClientID"", ""TargetType"", ""TargetID"", ""ReactionType"", ""CreatedAt"")
SELECT MIN(w.""WebsiteID""), w.""WebsiteClientID"", 1, v.""ProductID"", 1, MIN(i.""AddedAt"")
FROM ""WishlistItems"" i
JOIN ""Wishlists"" w ON w.""WishlistID"" = i.""WishlistID""
JOIN ""ProductVariants"" v ON v.""ProductVariantID"" = i.""ProductVariantID""
GROUP BY w.""WebsiteClientID"", v.""ProductID"";

UPDATE ""Products"" SET ""FavoriteCount"" = (
    SELECT COUNT(*) FROM ""ClientReactions"" r
    WHERE r.""TargetType"" = 1 AND r.""ReactionType"" = 1 AND r.""TargetID"" = ""Products"".""ProductID"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientReactions");

            migrationBuilder.DropColumn(
                name: "FavoriteCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "Pages");
        }
    }
}
