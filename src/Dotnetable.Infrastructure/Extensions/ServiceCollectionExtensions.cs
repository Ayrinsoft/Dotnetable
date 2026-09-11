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

        // Factory options are SCOPED so the live connection string is re-read from the store on
        // every new scope (HTTP request / Blazor circuit). That matters during first-run setup:
        // the Setup page writes the real connection, and the next scope must pick it up rather
        // than reusing a placeholder captured at startup.
        //
        // AppDbContext itself is TRANSIENT (not scoped). Blazor Server runs layout + page + nav
        // OnInitializedAsync concurrently inside one circuit scope. A single scoped DbContext was
        // then shared by every service and concurrent queries threw:
        //   "A second operation was started on this context instance..."
        // Transient means each consumer (typically a scoped service) gets its own instance when
        // constructed, so parallel component init no longer shares one context. For brand-new
        // services prefer IDbContextFactory + short-lived contexts (see DbContextFactoryExtensions)
        // so even two concurrent methods on the same service stay safe.
        services.AddDbContextFactory<AppDbContext>((sp, options) =>
            ConfigureFromStore(options, sp.GetRequiredService<IDatabaseConfigStore>()),
            ServiceLifetime.Scoped);

        // AddDbContextFactory also registers AppDbContext as Scoped (same lifetime as the factory).
        // That reintroduces the shared-context race in Blazor — strip it and re-register Transient.
        for (var i = services.Count - 1; i >= 0; i--)
        {
            if (services[i].ServiceType == typeof(AppDbContext))
                services.RemoveAt(i);
        }

        services.AddTransient<AppDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        services.AddSingleton<TranslationCache>();
        services.AddSingleton<LanguageCatalogCache>();
        // Self-registering localization keys: pages buffer unknown keys, a background service inserts them.
        services.AddSingleton<PendingTranslationKeys>();
        services.AddHostedService<TranslationKeyFlushService>();

        // Scheduled work. Registered here so whichever host runs it gets both; they are idempotent
        // and safe to have running in more than one process (each pass re-reads what is still due).
        //
        // OrderExpiryService is the other half of the checkout transaction: checkout reserves stock
        // before payment, and without this nothing ever gives it back for orders that are abandoned.
        services.AddHostedService<OrderExpiryService>();
        services.AddHostedService<MaintenanceService>();
        services.AddHostedService<Marketplace.MarketplaceSyncService>();
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
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

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
        services.AddScoped<IPriceListService, PriceListService>();

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
        services.AddScoped<ITaxReportService, TaxReportService>();
        services.AddScoped<ISettlementService, SettlementService>();
        services.AddScoped<IFinancialLedgerService, FinancialLedgerService>();
        services.AddScoped<IChartOfAccountService, ChartOfAccountService>();
        services.AddScoped<IJournalService, JournalService>();
        services.AddScoped<IAccountingReportService, AccountingReportService>();
        services.AddScoped<IGlProjector, GlProjector>();
        services.AddScoped<IFiscalPeriodService, FiscalPeriodService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IStockDocumentService, StockDocumentService>();
        services.AddScoped<ICustomerReturnService, CustomerReturnService>();
        services.AddScoped<IRecordAttachmentService, RecordAttachmentService>();
        services.AddScoped<IHrService, HrService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<ITaxPeriodService, TaxPeriodService>();

        // E-commerce: the website's own banks/bank accounts (manual/offline payment receiving).
        services.AddScoped<IBankService, BankService>();
        services.AddScoped<IBankAccountService, BankAccountService>();

        // E-commerce: shopping cart (guest + signed-in customer).
        services.AddScoped<ICartService, CartService>();

        // E-commerce: checkout orchestration + order lifecycle.
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IDigitalDeliveryService, DigitalDeliveryService>();
        services.AddScoped<ISupportDeskService, SupportDeskService>();

        // E-commerce: payments (wallet debit + manual bank transfer) and refunds.
        services.AddScoped<IPaymentService, PaymentService>();

        // Live admin work queues (dashboard / notifications task panel).
        services.AddScoped<IAdminTaskService, AdminTaskService>();
        services.AddScoped<IStaffTaskService, StaffTaskService>();

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

        services.AddScoped<AdvertisementService>();
        services.AddScoped<IAdvertisementService>(sp => new CachedAdvertisementService(
            sp.GetRequiredService<AdvertisementService>(), sp.GetRequiredService<ICacheService>(),
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
        // SMS gateways: one registration per provider, resolved per website from WebsiteSmsSettings
        // (same shape as the storage backends above). Adding a gateway is a new ISmsProvider class;
        // GenericHttpSmsProvider covers panels that need no code at all.
        services.AddScoped<ISmsProvider, Sms.KavenegarSmsProvider>();
        services.AddScoped<ISmsProvider, Sms.SmsIrProvider>();
        services.AddScoped<ISmsProvider, Sms.MelliPayamakSmsProvider>();
        services.AddScoped<ISmsProvider, Sms.GhasedakSmsProvider>();
        services.AddScoped<ISmsProvider, Sms.IpPanelSmsProvider>();
        services.AddScoped<ISmsProvider, Sms.TwilioSmsProvider>();
        services.AddScoped<ISmsProvider, Sms.VonageSmsProvider>();
        services.AddScoped<ISmsProvider, Sms.GenericHttpSmsProvider>();
        services.AddScoped<ISmsProviderRegistry, Sms.SmsProviderRegistry>();
        services.AddScoped<ISmsSender, Sms.SmsSender>();
        services.AddScoped<ISmsSettingService, Sms.SmsSettingService>();

        // Marketplace / product search engine channels, resolved per website from MarketplaceChannels.
        // GoogleShopping emits Google's own RSS spec; GenericFeed covers Torob, Emalls and any other
        // engine that crawls a document, and GenericApi covers Digikala/Basalam-style pushes — both
        // configured from the admin panel, since those engines publish their schema to sellers only.
        services.AddScoped<IMarketplaceProvider, Marketplace.GoogleShoppingFeedProvider>();
        services.AddScoped<IMarketplaceProvider, Marketplace.GenericFeedProvider>();
        services.AddScoped<IMarketplaceProvider, Marketplace.GenericApiMarketplaceProvider>();
        services.AddScoped<IMarketplaceProviderRegistry, Marketplace.MarketplaceProviderRegistry>();
        services.AddScoped<IMarketplaceProductSource, Marketplace.MarketplaceProductSource>();
        services.AddScoped<IMarketplaceChannelService, Marketplace.MarketplaceChannelService>();

        // Online payment gateways: one registration per PSP, resolved per website from PaymentGateways.
        // Adding a gateway is a new IPaymentGatewayProvider class and nothing else; GenericRedirect
        // covers panels that can be described in configuration alone.
        services.AddScoped<IPaymentGatewayProvider, Payments.ZarinpalGatewayProvider>();
        services.AddScoped<IPaymentGatewayProvider, Payments.ZibalGatewayProvider>();
        services.AddScoped<IPaymentGatewayProvider, Payments.IdPayGatewayProvider>();
        services.AddScoped<IPaymentGatewayProvider, Payments.NextPayGatewayProvider>();
        services.AddScoped<IPaymentGatewayProvider, Payments.PayIrGatewayProvider>();
        services.AddScoped<IPaymentGatewayProvider, Payments.PayPingGatewayProvider>();
        services.AddScoped<IPaymentGatewayProvider, Payments.StripeGatewayProvider>();
        services.AddScoped<IPaymentGatewayProvider, Payments.PayPalGatewayProvider>();
        services.AddScoped<IPaymentGatewayProvider, Payments.GenericRedirectGatewayProvider>();
        services.AddScoped<IPaymentGatewayProviderRegistry, Payments.PaymentGatewayProviderRegistry>();
        services.AddScoped<IOnlinePaymentService, Payments.OnlinePaymentService>();
        services.AddSingleton<IWhatsAppSender, NoOpWhatsAppSender>();

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
        // Do not fail MigrateAsync when the compiled model is slightly ahead of the last snapshot.
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
