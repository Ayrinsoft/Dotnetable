using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dotnetable.Migrations.MySql;

/// <summary>Design-time factory so `dotnet ef` generates MySQL/MariaDB migrations into this assembly.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DOTNETABLE_MIGRATIONS_CONNECTION")
            ?? "Server=localhost;Port=3306;Database=Dotnetable;User Id=root;Password=;";

        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UseMySQL(connectionString, mysql => mysql.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
        return new AppDbContext(options.Options);
    }
}
