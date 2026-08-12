using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <summary>
    /// Brings <c>AppDbContextModelSnapshot</c> in line with the current model after a stretch of
    /// raw-SQL migrations / Schema Compare updates that did not refresh the EF snapshot.
    /// Schema itself is already applied on the test database — Up/Down are intentionally empty
    /// so <c>dotnet ef database update</c> only records this migration in __EFMigrationsHistory.
    /// </summary>
    public partial class SyncModelSnapshot_20260812 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: columns/tables already exist via Schema Compare + prior SQL migrations.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: snapshot-only sync; do not tear down live schema.
        }
    }
}
