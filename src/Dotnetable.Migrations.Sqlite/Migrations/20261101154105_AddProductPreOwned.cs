using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddProductPreOwned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPreOwned",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                comment: "Used item owned by one store. On a platform, other websites may create only this kind of product.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPreOwned",
                table: "Products");
        }
    }
}
