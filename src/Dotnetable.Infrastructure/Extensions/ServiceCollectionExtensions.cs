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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(options => ConfigureProvider(options, provider, connectionString));
        services.AddDbContextFactory<AppDbContext>(options => ConfigureProvider(options, provider, connectionString));

        services.AddSingleton<TranslationCache>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IWebsiteService, WebsiteService>();
        services.AddScoped<ISetupService, SetupService>();
        services.AddScoped<IPasswordHasher<Member>, PasswordHasher<Member>>();

        services.AddApplication();

        return services;
    }

    private static void ConfigureProvider(DbContextOptionsBuilder options, string provider, string connectionString)
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
