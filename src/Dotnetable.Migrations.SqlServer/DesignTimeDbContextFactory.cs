using System.Text.Json;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dotnetable.Migrations.SqlServer;

/// <summary>Design-time factory so `dotnet ef` generates SQL Server migrations into this assembly.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DOTNETABLE_MIGRATIONS_CONNECTION")
            ?? TryReadConnectionFromNearbySettings()
            ?? "Server=.;Database=Dotnetable;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
        return new AppDbContext(options.Options);
    }

    private static string? TryReadConnectionFromNearbySettings()
    {
        // Walk up from the migrations project to find Admin/API connection strings used at runtime.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 8 && dir is not null; i++, dir = dir.Parent)
        {
            foreach (var relative in new[]
                     {
                         Path.Combine("src", "Dotnetable.Admin", "appsettings.json"),
                         Path.Combine("src", "Dotnetable.Admin", "localsettings.json"),
                         Path.Combine("src", "Dotnetable.API", "appsettings.json"),
                         Path.Combine("src", "Dotnetable.API", "localsettings.json"),
                     })
            {
                var path = Path.Combine(dir.FullName, relative);
                if (!File.Exists(path)) continue;
                try
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(path));
                    // Admin localsettings: { "Database": { "ConnectionString": "..." } }
                    if (doc.RootElement.TryGetProperty("Database", out var db)
                        && db.TryGetProperty("ConnectionString", out var dbCs)
                        && dbCs.GetString() is { Length: > 0 } fromDb)
                        return fromDb;
                    if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs)
                        && cs.TryGetProperty("DefaultConnection", out var def)
                        && def.GetString() is { Length: > 0 } s)
                        return s;
                }
                catch
                {
                    // ignore malformed local files
                }
            }
        }

        return null;
    }
}
