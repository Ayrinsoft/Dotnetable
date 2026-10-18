using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Migrations.Sqlite.Migrations;

public partial class AddQuizzes
{
    /// <summary>
    /// SQLite can add a foreign key only by rebuilding the table, and that rebuild reads the
    /// migration's target model. This migration is the latest one, so the current snapshot is that model.
    /// The rebuild copies every existing row into the new table.
    /// </summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
        => new AppDbContextModelSnapshot().CopyTo(modelBuilder);
}
