using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20261017092015_VisitCardExtraPhones")]
    public partial class VisitCardExtraPhones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone2",
                table: "AuthorVisitCards",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone2Title",
                table: "AuthorVisitCards",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone3",
                table: "AuthorVisitCards",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone3Title",
                table: "AuthorVisitCards",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone2",
                table: "AuthorVisitCards");

            migrationBuilder.DropColumn(
                name: "Phone2Title",
                table: "AuthorVisitCards");

            migrationBuilder.DropColumn(
                name: "Phone3",
                table: "AuthorVisitCards");

            migrationBuilder.DropColumn(
                name: "Phone3Title",
                table: "AuthorVisitCards");
        }
    }
}
