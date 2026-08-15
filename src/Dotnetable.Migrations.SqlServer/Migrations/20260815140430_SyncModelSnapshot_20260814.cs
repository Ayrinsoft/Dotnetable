using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <summary>
    /// Brings <c>AppDbContextModelSnapshot</c> in line with the current model after
    /// <c>20260814140000_VendorSettlementCurrency</c> (raw SQL, no Designer/snapshot).
    /// Schema is already on the test database — Up/Down are empty so
    /// <c>dotnet ef database update</c> only records this row in __EFMigrationsHistory.
    /// </summary>
    public partial class SyncModelSnapshot_20260814 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
