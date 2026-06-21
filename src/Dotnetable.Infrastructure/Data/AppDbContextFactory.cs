using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dotnetable.Infrastructure.Data;

/// <summary>
/// Design-time factory used by `dotnet ef` to scaffold/generate migrations. Migrations target
/// MariaDB/MySQL; no live connection is required to add a migration. Override the connection for
/// a manual `database update` via the DOTNETABLE_MIGRATIONS_CONNECTION environment variable.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DOTNETABLE_MIGRATIONS_CONNECTION")
            ?? "Server=localhost;Port=3306;Database=Dotnetable;User Id=root;Password=;";

        var options = new DbContextOptionsBuilder<AppDbContext>();
        ServiceCollectionExtensions.ConfigureProvider(options, "MariaDB", connectionString);
        return new AppDbContext(options.Options);
    }
}
