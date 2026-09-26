using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogAgentKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CatalogAgentKey",
                table: "Websites",
                type: "uniqueidentifier",
                nullable: true,
                comment: "Secret for POST /api/product-agent. Not the public storefront key.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CatalogAgentKey",
                table: "Websites");
        }
    }
}
