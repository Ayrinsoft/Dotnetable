using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public class ProductCodePrefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductCodePrefix",
                table: "Websites",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "DN",
                comment: "1–3 letter product code prefix; codes are {prefix}-{ProductID}.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductCodePrefix",
                table: "Websites");
        }
    }
}
