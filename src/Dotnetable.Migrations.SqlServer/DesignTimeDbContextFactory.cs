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
            ?? "Server=.;Database=Dotnetable;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
        return new AppDbContext(options.Options);
    }
}
