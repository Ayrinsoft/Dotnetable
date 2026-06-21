using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dotnetable.Migrations.PostgreSql;

/// <summary>Design-time factory so `dotnet ef` generates PostgreSQL migrations into this assembly.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DOTNETABLE_MIGRATIONS_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=Dotnetable;Username=postgres;Password=postgres;";

        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
        return new AppDbContext(options.Options);
    }
}
