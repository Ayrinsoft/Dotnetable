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
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWebsiteService, WebsiteService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

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
                options.UseMySQL(connectionString);
                break;

            default:
                throw new NotSupportedException(
                    $"Database provider '{provider}' is not supported. Use: SqlServer, PostgreSQL, MySQL, MariaDB");
        }
    }
}
