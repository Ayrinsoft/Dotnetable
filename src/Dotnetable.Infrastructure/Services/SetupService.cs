using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class SetupService : ISetupService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IDatabaseConfigStore _configStore;
    private readonly IDatabaseProvisionerRegistry _provisioners;
    private readonly IPasswordHasher<Member> _hasher;

    public SetupService(
        IDbContextFactory<AppDbContext> contextFactory,
        IDatabaseConfigStore configStore,
        IDatabaseProvisionerRegistry provisioners,
        IPasswordHasher<Member> hasher)
    {
        _contextFactory = contextFactory;
        _configStore = configStore;
        _provisioners = provisioners;
        _hasher = hasher;
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

        // Build/upgrade the schema, then seed initial data in one transaction.
        await context.Database.MigrateAsync(ct);
        await SeedInitialDataAsync(context, request, ct);
    }

    private async Task SeedInitialDataAsync(AppDbContext context, SetupRequest request, CancellationToken ct)
    {
        // Providers are configured with EnableRetryOnFailure, so a user-initiated transaction must
        // run inside the execution strategy as a single retriable unit — otherwise EF throws.
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            // 1. Master website. As the first row inserted into an empty table it receives WebsiteID 1.
        var website = new Website
        {
            TradeName = request.TradeName,
            BrandName = request.BrandName,
            WebsiteAddress = request.WebsiteAddress,
            Manager = request.Manager,
            Mobile = request.Mobile,
            Email = request.WebsiteEmail,
            DefaultLanguageCode = request.DefaultLanguageCode,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            AuthCode = Guid.NewGuid(),
            Active = true,
            AllowAllIP = true,
            WebsiteType = 0,
        };
        context.Websites.Add(website);
        await context.SaveChangesAsync(ct);

        // 2. Seed permission roles.
        var roles = RoleKeys.All
            .Select(key => new Role { RoleKey = key, Description = key, Active = true })
            .ToList();
        context.Roles.AddRange(roles);
        await context.SaveChangesAsync(ct);

        // 3. Administrator policy with full access (every seeded role).
        var policy = new Policy { Title = "Administrators", Active = true };
        context.Policies.Add(policy);
        await context.SaveChangesAsync(ct);

        context.PolicyRoles.AddRange(roles.Select(r => new PolicyRole
        {
            PolicyID = policy.PolicyID,
            RoleID = r.RoleID,
            Active = true,
        }));
        await context.SaveChangesAsync(ct);

        // 4. First administrator member, bound to the master website.
        var member = new Member
        {
            WebsiteID = website.WebsiteID,
            PolicyID = policy.PolicyID,
            Username = request.Username,
            Email = request.Email,
            Givenname = request.Givenname,
            Surname = request.Surname,
            CellphoneNumber = string.Empty,
            CountryCode = string.Empty,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            Active = true,
        };
        member.Password = _hasher.HashPassword(member, request.Password);
        context.Members.Add(member);
        await context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        });
    }
}
