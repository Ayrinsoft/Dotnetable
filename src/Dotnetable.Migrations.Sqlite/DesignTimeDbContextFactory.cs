using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dotnetable.Migrations.Sqlite;

/// <summary>Design-time factory so <c>dotnet ef</c> generates SQLite migrations into this assembly.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Do not read Admin/API settings here. Those files point at SQL Server and would be
        // the wrong target for a SQLite design-time build.
        var connectionString = Environment.GetEnvironmentVariable("DOTNETABLE_MIGRATIONS_CONNECTION")
            ?? "Data Source=dotnetable.sqlite";

        var options = new DbContextOptionsBuilder<AppDbContext>();
        SqliteDatabaseSetup.Configure(options, connectionString);
        return new AppDbContext(options.Options);
    }
}
