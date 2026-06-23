using Dotnetable.Application.Extensions;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Interfaces;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Provisioning;
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
        var settingsPath = Path.Combine(contentRootPath, "localsettings.json");

        // One writable JSON file owns everything that must persist outside the database: the live
        // connection AND the anti-bot security settings. A single instance is shared by both
        // interfaces so there is never more than one writer to the file.
        services.AddSingleton<LocalSettingsStore>(_ =>
            new LocalSettingsStore(settingsPath, defaultProvider, defaultConnection));
        services.AddSingleton<IDatabaseConfigStore>(sp => sp.GetRequiredService<LocalSettingsStore>());
        services.AddSingleton<IAppSettingsStore>(sp => sp.GetRequiredService<LocalSettingsStore>());

        // Register the factory with SCOPED options so the live connection string is re-read from
        // the store on every new scope (HTTP request / Blazor circuit). This matters during
        // first-run setup: the Setup page writes the real connection to the store, and the next
        // scope must pick it up rather than reusing the placeholder captured at startup. The
        // scoped DbContext is derived from the factory so both share one options lifetime and avoid
        // the singleton/scoped conflict that registering AddDbContext separately would cause.
        services.AddDbContextFactory<AppDbContext>((sp, options) =>
            ConfigureFromStore(options, sp.GetRequiredService<IDatabaseConfigStore>()),
            ServiceLifetime.Scoped);
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

        // Login/forgot-password protection + email.
        services.AddMemoryCache();
        services.AddSingleton<IHumanVerificationService, HumanVerificationService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();

        // Provider-specific connection test / database creation used by the Setup page.
        services.AddSingleton<IDatabaseProvisioner, SqlServerProvisioner>();
        services.AddSingleton<IDatabaseProvisioner, PostgreSqlProvisioner>();
        services.AddSingleton<IDatabaseProvisioner, MySqlProvisioner>();
        services.AddSingleton<IDatabaseProvisionerRegistry, DatabaseProvisionerRegistry>();

        services.AddApplication();

        return services;
    }

    private static void ConfigureFromStore(DbContextOptionsBuilder options, IDatabaseConfigStore store) =>
        ConfigureProvider(options, store.Provider, store.ConnectionString ?? PlaceholderConnection);

    // Migrations are provider-specific, so each provider keeps its own migrations assembly/project.
    public const string SqlServerMigrations = "Dotnetable.Migrations.SqlServer";
    public const string MySqlMigrations = "Dotnetable.Migrations.MySql";
    public const string PostgreSqlMigrations = "Dotnetable.Migrations.PostgreSql";

    internal static void ConfigureProvider(DbContextOptionsBuilder options, string provider, string connectionString)
    {
        switch (provider.ToLowerInvariant())
        {
            case "sqlserver":
                options.UseSqlServer(connectionString, sql =>
                {
                    sql.EnableRetryOnFailure(3);
                    sql.MigrationsAssembly(SqlServerMigrations);
                });
                break;

            case "postgresql":
            case "postgres":
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(3);
                    npgsql.MigrationsAssembly(PostgreSqlMigrations);
                });
                break;

            case "mysql":
            case "mariadb":
                // MariaDB speaks the MySQL protocol; the Oracle MySql.EntityFrameworkCore
                // provider (replacing the removed Pomelo/MariaDB package) handles both.
                options.UseMySQL(connectionString, mysql => mysql.MigrationsAssembly(MySqlMigrations));
                break;

            default:
                throw new NotSupportedException(
                    $"Database provider '{provider}' is not supported. Use: SqlServer, PostgreSQL, MySQL, MariaDB");
        }
    }
}
