using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260812090000_ProductCodePrefix")]
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
                comment: "1â€“3 letter product code prefix; codes are {prefix}-{ProductID}.");
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
