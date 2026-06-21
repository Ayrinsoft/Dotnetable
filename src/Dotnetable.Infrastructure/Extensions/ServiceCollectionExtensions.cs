using Dotnetable.Application.Extensions;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Interfaces;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Repositories;
using Dotnetable.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetable.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    // Used before the database is configured, so context registration never throws at startup.
    private const string PlaceholderConnection = "Server=localhost;Database=__unconfigured;User Id=root;Password=;";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        var defaultProvider = configuration["Database:Provider"] ?? "MariaDB";
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        var dbSettingsPath = Path.Combine(contentRootPath, "dbsettings.json");

        // Single source of truth for the live connection; seeded from appsettings, overridden by dbsettings.json.
        services.AddSingleton<IDatabaseConfigStore>(_ =>
            new DatabaseConfigStore(dbSettingsPath, defaultProvider, defaultConnection));

        // Register the factory (its options are singleton and read the live connection from the
        // store) and derive the scoped DbContext from it, so both can coexist without the
        // singleton/scoped options conflict that registering AddDbContext separately would cause.
        services.AddDbContextFactory<AppDbContext>((sp, options) =>
            ConfigureFromStore(options, sp.GetRequiredService<IDatabaseConfigStore>()));
        services.AddScoped<AppDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        services.AddSingleton<TranslationCache>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IWebsiteService, WebsiteService>();
        services.AddScoped<ISetupService, SetupService>();
        services.AddScoped<IDatabaseUpdateService, DatabaseUpdateService>();
        services.AddScoped<IPasswordHasher<Member>, PasswordHasher<Member>>();

        services.AddApplication();

        return services;
    }

    private static void ConfigureFromStore(DbContextOptionsBuilder options, IDatabaseConfigStore store) =>
        ConfigureProvider(options, store.Provider, store.ConnectionString ?? PlaceholderConnection);

    internal static void ConfigureProvider(DbContextOptionsBuilder options, string provider, string connectionString)
    {
        switch (provider.ToLowerInvariant())
        {
            case "sqlserver":
                options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(3));
                break;

            case "postgresql":
            case "postgres":
                options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure(3));
                break;

            case "mysql":
            case "mariadb":
                // MariaDB speaks the MySQL protocol; the Oracle MySql.EntityFrameworkCore
                // provider (replacing the removed Pomelo/MariaDB package) handles both.
                options.UseMySQL(connectionString);
                break;

            default:
                throw new NotSupportedException(
                    $"Database provider '{provider}' is not supported. Use: SqlServer, PostgreSQL, MySQL, MariaDB");
        }
    }
}
