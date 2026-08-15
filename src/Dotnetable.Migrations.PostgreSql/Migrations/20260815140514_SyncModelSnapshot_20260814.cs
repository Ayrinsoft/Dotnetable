using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <summary>
    /// Snapshot-only sync after a stretch of raw-SQL migrations that did not refresh
    /// <c>AppDbContextModelSnapshot</c>. Schema itself is applied by those prior migrations.
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
