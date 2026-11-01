using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddPostContentEditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SupportsStableKey",
                table: "PostTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Opt-in. Posts of this type may keep a stable key. Blog types leave this off.");

            migrationBuilder.AddColumn<bool>(
                name: "SupportsVersioning",
                table: "PostTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Opt-in. Posts of this type may have release-scoped editions that share a stable key.");

            migrationBuilder.AddColumn<string>(
                name: "AppliesFrom",
                table: "Posts",
                type: "character varying(40)",
                unicode: false,
                maxLength: 40,
                nullable: true,
                comment: "First content release this edition covers. Null means unversioned or every release.");

            migrationBuilder.AddColumn<string>(
                name: "AppliesTo",
                table: "Posts",
                type: "character varying(40)",
                unicode: false,
                maxLength: 40,
                nullable: true,
                comment: "Last content release this edition covers, inclusive. Null means the edition is still open.");

            migrationBuilder.AddColumn<string>(
                name: "StableKey",
                table: "Posts",
                type: "character varying(80)",
                unicode: false,
                maxLength: 80,
                nullable: true,
                comment: "Stable reference for one logical post. Null unless the post type opts in. Immutable once published.");

            migrationBuilder.CreateTable(
                name: "ContentVersions",
                columns: table => new
                {
                    ContentVersionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", unicode: false, maxLength: 40, nullable: false, comment: "Content release label such as 1.2. Not an API version and not a product version."),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsLatest = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "The release used when a reader does not ask for a version."),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Hidden from the public version list until published.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentVersions", x => x.ContentVersionID);
                    table.ForeignKey(
                        name: "FK_ContentVersions_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Posts_StableEdition",
                table: "Posts",
                columns: new[] { "WebsiteID", "PostTypeID", "StableKey", "AppliesFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentVersions_WebsiteID",
                table: "ContentVersions",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "UQ_ContentVersions_Website_Name",
                table: "ContentVersions",
                columns: new[] { "WebsiteID", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentVersions");

            migrationBuilder.DropIndex(
                name: "UQ_Posts_StableEdition",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "SupportsStableKey",
                table: "PostTypes");

            migrationBuilder.DropColumn(
                name: "SupportsVersioning",
                table: "PostTypes");

            migrationBuilder.DropColumn(
                name: "AppliesFrom",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "AppliesTo",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "StableKey",
                table: "Posts");
        }
    }
}
