using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class SetupService : ISetupService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IDatabaseConfigStore _configStore;
    private readonly IAppSettingsStore _appSettings;
    private readonly IDatabaseProvisionerRegistry _provisioners;
    private readonly IInitialDataSeeder _seeder;

    public SetupService(
        IDbContextFactory<AppDbContext> contextFactory,
        IDatabaseConfigStore configStore,
        IAppSettingsStore appSettings,
        IDatabaseProvisionerRegistry provisioners,
        IInitialDataSeeder seeder)
    {
        _contextFactory = contextFactory;
        _configStore = configStore;
        _appSettings = appSettings;
        _provisioners = provisioners;
        _seeder = seeder;
    }

    public async Task<bool> IsSetupCompletedAsync(CancellationToken ct = default)
    {
        if (!_configStore.IsConfigured) return false;

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            if (!await context.Database.CanConnectAsync(ct)) return false;
            return await context.Websites.AnyAsync(ct);
        }
        catch
        {
            // Unreachable database, missing schema, etc. — treat as not yet set up.
            return false;
        }
    }

    public Task<ConnectionTestResult> TestConnectionAsync(DatabaseConnectionInfo db, CancellationToken ct = default) =>
        _provisioners.Get(db.Provider).TestConnectionAsync(db, ct);

    public async Task CompleteSetupAsync(SetupRequest request, CancellationToken ct = default)
    {
        if (await IsSetupCompletedAsync(ct))
            throw new InvalidOperationException("Setup has already been completed.");

        var provisioner = _provisioners.Get(request.Database.Provider);

        var test = await provisioner.TestConnectionAsync(request.Database, ct);
        if (!test.Success)
            throw new InvalidOperationException($"Database connection failed: {test.Message}");

        if (request.Database.CreateDatabaseIfMissing)
            await provisioner.CreateDatabaseAsync(request.Database, ct);

        // Persist the connection so the DbContext (and the API) bind to it from now on.
        var connectionString = provisioner.BuildConnectionString(request.Database, includeDatabase: true);
        await _configStore.SaveAsync(request.Database.Provider, connectionString, ct);

        // The shared DbContextFactory caches its options as a singleton, and those options were
        // very likely materialized earlier in this request (IsSetupCompletedAsync above) with the
        // placeholder connection string. Build a fresh context bound to the just-saved connection
        // so migrate/seed run against the real target rather than the stale placeholder.
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        ServiceCollectionExtensions.ConfigureProvider(optionsBuilder, request.Database.Provider, connectionString);
        await using var context = new AppDbContext(optionsBuilder.Options);

        // Persist the optional anti-bot keys collected on the form (empty = math captcha fallback).
        await _appSettings.SaveSecurityAsync(new SecuritySettings
        {
            TurnstileSiteKey = request.TurnstileSiteKey,
            TurnstileSecretKey = request.TurnstileSecretKey,
            CaptchaMode = CaptchaMode.Auto,
        }, ct);

        // Build/upgrade the schema, then seed initial data in one transaction.
        await context.Database.MigrateAsync(ct);
        await _seeder.SeedAsync(context, request, ct);
    }

    public async Task SyncRoleCatalogAsync(CancellationToken ct = default)
    {
        if (!_configStore.IsConfigured) return;

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        if (!await context.Database.CanConnectAsync(ct)) return;

        var existing = (await context.Roles.Select(r => r.RoleKey).ToListAsync(ct)).ToHashSet();
        var missing = RoleCatalog.All.Where(def => !existing.Contains(def.Key)).ToList();
        if (missing.Count == 0) return;

        context.Roles.AddRange(missing.Select(def => new Role
        {
            RoleKey = def.Key,
            Description = def.Description,
            Category = (byte)def.Category,
            Active = true,
        }));
        await context.SaveChangesAsync(ct);
    }
}
