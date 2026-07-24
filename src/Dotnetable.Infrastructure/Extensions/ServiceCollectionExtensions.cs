using Dotnetable.Application.Authorization;
using Dotnetable.Application.Extensions;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Interfaces;
using Dotnetable.Infrastructure.Caching;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Provisioning;
using Dotnetable.Infrastructure.Repositories;
using Dotnetable.Infrastructure.Services;
using Dotnetable.Infrastructure.Services.Caching;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        services.AddSingleton<LanguageCatalogCache>();
        // Self-registering localization keys: pages buffer unknown keys, a background service inserts them.
        services.AddSingleton<PendingTranslationKeys>();
        services.AddHostedService<TranslationKeyFlushService>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IWebsiteClientService, WebsiteClientService>();
        services.AddScoped<IWebsiteClientAddressService, WebsiteClientAddressService>();
        services.AddScoped<IWebsiteClientAuthService, WebsiteClientAuthService>();
        services.AddScoped<IWebsiteService, WebsiteService>();
        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ILoginLogService, LoginLogService>();
        services.AddScoped<IInitialDataSeeder, InitialDataSeeder>();
        services.AddScoped<ISetupService, SetupService>();
        services.AddScoped<IDatabaseUpdateService, DatabaseUpdateService>();
        services.AddScoped<IPasswordHasher<Member>, PasswordHasher<Member>>();
        services.AddScoped<IPasswordHasher<WebsiteClient>, PasswordHasher<WebsiteClient>>();

        // JWT issuance for API / website-client logins. Bound from the "Jwt" config section; the
        // signing key is only required by hosts that actually issue or validate tokens (the API).
        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
        services.AddSingleton(jwtSettings);
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Media library: pluggable CDN storage backends + file/album/tag management.
        services.AddHttpClient(); // used by HTTP-based providers (BunnyCDN)
        services.AddScoped<IFileStorageProvider, Storage.ArvanStorageProvider>();
        services.AddScoped<IFileStorageProvider, Storage.DropboxStorageProvider>();
        // On-host disk storage needs the content root to resolve its upload directory.
        services.AddScoped<IFileStorageProvider>(_ => new Storage.LocalStorageProvider(contentRootPath));
        // S3-compatible family (one shared implementation, distinct selectable providers).
        services.AddScoped<IFileStorageProvider, Storage.AwsS3StorageProvider>();
        services.AddScoped<IFileStorageProvider, Storage.CloudflareR2StorageProvider>();
        services.AddScoped<IFileStorageProvider, Storage.BackBlazeStorageProvider>();
        services.AddScoped<IFileStorageProvider, Storage.MinioStorageProvider>();
        services.AddScoped<IFileStorageProvider, Storage.AzureBlobStorageProvider>();
        services.AddScoped<IFileStorageProvider, Storage.CloudinaryStorageProvider>();
        services.AddScoped<IFileStorageProvider, Storage.BunnyStorageProvider>();
        services.AddScoped<IFileStorageProviderRegistry, Storage.FileStorageProviderRegistry>();
        services.AddScoped<IStorageSettingService, StorageSettingService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IWebsiteSettingService, WebsiteSettingService>();
        services.AddScoped<IContactMessageService, ContactMessageService>();

        // E-commerce: currency master list + per-website exchange rates + USD conversion helper.
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<ICurrencyRateService, CurrencyRateService>();
        services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();

        // E-commerce: catalog (categories, attributes, brands, vendors, products+variants).
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IAttributeDefinitionService, AttributeDefinitionService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IWarrantyService, WarrantyService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IVendorProductService, VendorProductService>();
        services.AddScoped<IVendorCreditService, VendorCreditService>();
        services.AddScoped<IProductService, ProductService>();

        // E-commerce: inventory / stock / suppliers.
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IStockMovementService, StockMovementService>();
        services.AddScoped<ISupplierService, SupplierService>();

        // E-commerce: customer wallets, withdrawals, bank accounts.
        services.AddScoped<IClientWalletService, ClientWalletService>();
        services.AddScoped<IClientWalletWithdrawalService, ClientWalletWithdrawalService>();
        services.AddScoped<IClientBankAccountService, ClientBankAccountService>();

        // E-commerce: coupons, shipping, tax.
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IShippingService, ShippingService>();
        services.AddScoped<ITaxService, TaxService>();

        // E-commerce: the website's own banks/bank accounts (manual/offline payment receiving).
        services.AddScoped<IBankService, BankService>();
        services.AddScoped<IBankAccountService, BankAccountService>();

        // E-commerce: shopping cart (guest + signed-in customer).
        services.AddScoped<ICartService, CartService>();

        // E-commerce: checkout orchestration + order lifecycle.
        services.AddScoped<IOrderService, OrderService>();

        // E-commerce: payments (wallet debit + manual bank transfer) and refunds.
        services.AddScoped<IPaymentService, PaymentService>();

        // E-commerce: product reviews and Q&A.
        services.AddScoped<IProductReviewService, ProductReviewService>();
        services.AddScoped<IProductQuestionService, ProductQuestionService>();

        // E-commerce: customer wishlist.
        services.AddScoped<IWishlistService, WishlistService>();

        // Content read caching: IMemoryCache-backed, tag-invalidated on write (menu/category/page/post).
        // Menu/Category/Page/Post are registered under their concrete type too so the Cached* decorator
        // can hold the real implementation while IxxxService resolves to the decorator.
        services.AddMemoryCache();
        var cacheOptions = new CacheOptions();
        configuration.GetSection(CacheOptions.SectionName).Bind(cacheOptions);
        services.AddSingleton(cacheOptions);
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.TryAddSingleton<ICacheInvalidationNotifier, NoOpCacheInvalidationNotifier>();

        services.AddScoped<MenuService>();
        services.AddScoped<IMenuService>(sp => new CachedMenuService(
            sp.GetRequiredService<MenuService>(), sp.GetRequiredService<ICacheService>(),
            sp.GetRequiredService<ICacheInvalidationNotifier>(), sp.GetRequiredService<CacheOptions>()));

        services.AddScoped<SlideshowService>();
        services.AddScoped<ISlideshowService>(sp => new CachedSlideshowService(
            sp.GetRequiredService<SlideshowService>(), sp.GetRequiredService<ICacheService>(),
            sp.GetRequiredService<ICacheInvalidationNotifier>(), sp.GetRequiredService<CacheOptions>()));

        // Content: posts, pages, taxonomy (categories/tags/post types) and redirects.
        services.AddScoped<IPostTypeService, PostTypeService>();
        services.AddScoped<ITagService, TagService>();

        services.AddScoped<CategoryService>();
        services.AddScoped<ICategoryService>(sp => new CachedCategoryService(
            sp.GetRequiredService<CategoryService>(), sp.GetRequiredService<ICacheService>(),
            sp.GetRequiredService<ICacheInvalidationNotifier>(), sp.GetRequiredService<CacheOptions>()));

        services.AddScoped<PostService>();
        services.AddScoped<IPostService>(sp => new CachedPostService(
            sp.GetRequiredService<PostService>(), sp.GetRequiredService<ICacheService>(),
            sp.GetRequiredService<ICacheInvalidationNotifier>(), sp.GetRequiredService<CacheOptions>()));

        services.AddScoped<PageService>();
        services.AddScoped<IPageService>(sp => new CachedPageService(
            sp.GetRequiredService<PageService>(), sp.GetRequiredService<ICacheService>(),
            sp.GetRequiredService<ICacheInvalidationNotifier>(), sp.GetRequiredService<CacheOptions>()));

        services.AddScoped<IRedirectService, RedirectService>();

        // Dynamic form / survey builder with response collection and reporting.
        services.AddScoped<IFormService, FormService>();

        // WordPress-style website theme packages (zip install / activate / export).
        services.AddScoped<IThemeService, ThemeService>();

        // Login/forgot-password protection + email.
        services.AddSingleton<IHumanVerificationService, HumanVerificationService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailAccountService, EmailAccountService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IAdminNotificationService, AdminNotificationService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        // SMS delivery for customer OTP/reset codes. No real gateway yet — the stub logs the message.
        services.AddSingleton<ISmsSender, NoOpSmsSender>();

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
        // Schema changes after the baseline InitialCreate are applied manually via the SQL project
        // (Dotnetable.Database). Do not fail MigrateAsync when the model has drifted from the last
        // EF snapshot — production updates are SQL scripts, not new EF migration files.
        options.ConfigureWarnings(w =>
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

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
