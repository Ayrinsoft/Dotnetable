using Dotnetable.Application.Security;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Text;
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
        // Two independent gates, because they fail in opposite directions.
        //
        // IsSetupCompletedAsync answers "does a website row exist?", and deliberately reports false
        // when the database is unreachable — which is right for the redirect, but wrong here: during
        // a database outage it would let an anonymous visitor re-run first-run setup on /setup,
        // point localsettings.json at a database of their own and create themselves an administrator.
        // The presence of a stored connection is the durable fact and survives the outage, so it is
        // checked first and on its own.
        if (_configStore.IsConfigured)
            throw new InvalidOperationException("Setup has already been completed.");

        if (await IsSetupCompletedAsync(ct))
            throw new InvalidOperationException("Setup has already been completed.");

        var adminPassword = PasswordPolicy.ValidateAdmin(
            request.Password, request.Username, request.Email);
        if (!adminPassword.Ok)
            throw new InvalidOperationException(adminPassword.Error);

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

        var existing = await context.Roles.ToListAsync(ct);
        var existingKeys = existing.Select(r => r.RoleKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = RoleCatalog.All.Where(def => !existingKeys.Contains(def.Key)).ToList();
        if (missing.Count > 0)
        {
            context.Roles.AddRange(missing.Select(def => new Role
            {
                RoleKey = def.Key,
                Description = def.Description,
                Category = (byte)def.Category,
                Active = true,
            }));
            await context.SaveChangesAsync(ct);
            existing = await context.Roles.ToListAsync(ct);
        }

        // New keys are inserted additively; Administrators must always receive every catalog
        // permission (same as first-run seed). A prior version that only topped up Roles left
        // warehouse / accounting / HR / support grants missing on the live admin policy.
        var catalogKeys = RoleCatalog.All.Select(d => d.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var catalogRoles = existing.Where(r => catalogKeys.Contains(r.RoleKey)).ToList();

        var adminPolicyIds = await context.Policies
            .Where(p => p.Title == DefaultPolicies.Administrators)
            .Select(p => p.PolicyID)
            .ToListAsync(ct);

        if (catalogRoles.Count > 0 && adminPolicyIds.Count > 0)
        {
            var granted = await context.PolicyRoles
                .Where(pr => adminPolicyIds.Contains(pr.PolicyID))
                .Select(pr => new { pr.PolicyID, pr.RoleID })
                .ToListAsync(ct);
            var grantedSet = granted.Select(g => (g.PolicyID, g.RoleID)).ToHashSet();

            var toAdd = new List<PolicyRole>();
            foreach (var policyId in adminPolicyIds)
            {
                foreach (var role in catalogRoles)
                {
                    if (grantedSet.Contains((policyId, role.RoleID))) continue;
                    toAdd.Add(new PolicyRole
                    {
                        PolicyID = policyId,
                        RoleID = role.RoleID,
                        Active = true,
                    });
                }
            }

            if (toAdd.Count > 0)
            {
                context.PolicyRoles.AddRange(toAdd);
                await context.SaveChangesAsync(ct);
            }
        }

        // Additive top-up of seeded staff templates (Warehouse / Sales / Finance / HR) so
        // existing sites pick up new keys such as tasks.view without hand-editing policies.
        var roleIdByKey = existing.ToDictionary(r => r.RoleKey, r => r.RoleID, StringComparer.OrdinalIgnoreCase);
        foreach (var (title, keys) in DefaultPolicies.StaffTemplates)
        {
            var staffPolicyIds = await context.Policies
                .Where(p => p.Title == title)
                .Select(p => p.PolicyID)
                .ToListAsync(ct);
            if (staffPolicyIds.Count == 0) continue;

            var staffGranted = await context.PolicyRoles
                .Where(pr => staffPolicyIds.Contains(pr.PolicyID))
                .Select(pr => new { pr.PolicyID, pr.RoleID })
                .ToListAsync(ct);
            var staffGrantedSet = staffGranted.Select(g => (g.PolicyID, g.RoleID)).ToHashSet();

            var staffAdd = new List<PolicyRole>();
            foreach (var policyId in staffPolicyIds)
            {
                foreach (var key in keys)
                {
                    if (!roleIdByKey.TryGetValue(key, out var roleId)) continue;
                    if (staffGrantedSet.Contains((policyId, roleId))) continue;
                    staffAdd.Add(new PolicyRole { PolicyID = policyId, RoleID = roleId, Active = true });
                    staffGrantedSet.Add((policyId, roleId));
                }
            }

            if (staffAdd.Count == 0) continue;
            context.PolicyRoles.AddRange(staffAdd);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task ReslugifyContentAsync(CancellationToken ct = default)
    {
        if (!_configStore.IsConfigured) return;

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        if (!await context.Database.CanConnectAsync(ct)) return;

        var categories = await context.Categories.Select(c => new { c.CategoryID, c.WebsiteID, c.Slug }).ToListAsync(ct);
        var categoryTranslations = await context.CategoryTranslations.Select(t => new { t.CategoryTranslationID, t.CategoryID, t.Slug }).ToListAsync(ct);
        await ReslugifyAsync(
            context,
            main: categories.Select(c => (c.CategoryID, c.WebsiteID, c.Slug)).ToList(),
            translations: categoryTranslations.Select(t => (t.CategoryTranslationID, t.CategoryID, t.Slug)).ToList(),
            maxLength: 200,
            updateMain: (id, slug) => { var e = new Category { CategoryID = id }; context.Categories.Attach(e); e.Slug = slug; context.Entry(e).Property(x => x.Slug).IsModified = true; },
            updateTranslation: (id, slug) => { var e = new CategoryTranslation { CategoryTranslationID = id }; context.CategoryTranslations.Attach(e); e.Slug = slug; context.Entry(e).Property(x => x.Slug).IsModified = true; },
            ct);

        var posts = await context.Posts.Select(p => new { p.PostID, p.WebsiteID, p.Slug }).ToListAsync(ct);
        var postTranslations = await context.PostTranslations.Select(t => new { t.PostTranslationID, t.PostID, t.Slug }).ToListAsync(ct);
        await ReslugifyAsync(
            context,
            main: posts.Select(p => (p.PostID, p.WebsiteID, p.Slug)).ToList(),
            translations: postTranslations.Select(t => (t.PostTranslationID, t.PostID, t.Slug)).ToList(),
            maxLength: 300,
            updateMain: (id, slug) => { var e = new Post { PostID = id }; context.Posts.Attach(e); e.Slug = slug; context.Entry(e).Property(x => x.Slug).IsModified = true; },
            updateTranslation: (id, slug) => { var e = new PostTranslation { PostTranslationID = id }; context.PostTranslations.Attach(e); e.Slug = slug; context.Entry(e).Property(x => x.Slug).IsModified = true; },
            ct);

        var pages = await context.Pages.Select(p => new { p.PageID, p.WebsiteID, p.Slug }).ToListAsync(ct);
        var pageTranslations = await context.PageTranslations.Select(t => new { t.PageTranslationID, t.PageID, t.Slug }).ToListAsync(ct);
        await ReslugifyAsync(
            context,
            main: pages.Select(p => (p.PageID, p.WebsiteID, p.Slug)).ToList(),
            translations: pageTranslations.Select(t => (t.PageTranslationID, t.PageID, t.Slug)).ToList(),
            maxLength: 300,
            updateMain: (id, slug) => { var e = new Page { PageID = id }; context.Pages.Attach(e); e.Slug = slug; context.Entry(e).Property(x => x.Slug).IsModified = true; },
            updateTranslation: (id, slug) => { var e = new PageTranslation { PageTranslationID = id }; context.PageTranslations.Attach(e); e.Slug = slug; context.Entry(e).Property(x => x.Slug).IsModified = true; },
            ct);

        var tags = await context.Tags.Select(t => new { t.TagID, t.WebsiteID, t.Slug }).ToListAsync(ct);
        var tagTranslations = await context.TagTranslations.Select(t => new { t.TagTranslationID, t.TagID, t.Slug }).ToListAsync(ct);
        await ReslugifyAsync(
            context,
            main: tags.Select(t => (t.TagID, t.WebsiteID, t.Slug)).ToList(),
            translations: tagTranslations.Select(t => (t.TagTranslationID, t.TagID, t.Slug)).ToList(),
            maxLength: 150,
            updateMain: (id, slug) => { var e = new Tag { TagID = id }; context.Tags.Attach(e); e.Slug = slug; context.Entry(e).Property(x => x.Slug).IsModified = true; },
            updateTranslation: (id, slug) => { var e = new TagTranslation { TagTranslationID = id }; context.TagTranslations.Attach(e); e.Slug = slug; context.Entry(e).Property(x => x.Slug).IsModified = true; },
            ct);
    }

    /// <summary>Shared re-slugify pass for one entity type: re-normalizes any main/translation row
    /// whose current slug isn't already in normalized form, resolving collisions per-website the
    /// same way a live Create/Update would (see <see cref="SlugGenerator"/>). <paramref name="main"/>
    /// rows carry their own WebsiteID; <paramref name="translations"/> carry their parent's id, which
    /// is resolved back to a WebsiteID via <paramref name="main"/> so a translation of an
    /// already-deleted parent (shouldn't happen, but Slug alone doesn't join) is skipped rather than
    /// throwing.</summary>
    private static async Task ReslugifyAsync(
        AppDbContext context,
        List<(int Id, int WebsiteID, string Slug)> main,
        List<(int Id, int ParentId, string Slug)> translations,
        int maxLength,
        Action<int, string> updateMain,
        Action<int, string> updateTranslation,
        CancellationToken ct)
    {
        var websiteByParentId = main.ToDictionary(r => r.Id, r => r.WebsiteID);

        var mainWork = main
            .Select(r => (r.Id, r.WebsiteID, r.Slug, Normalized: SlugGenerator.Normalize(r.Slug, maxLength)))
            .ToList();
        var translationWork = translations
            .Where(t => websiteByParentId.ContainsKey(t.ParentId))
            .Select(t => (t.Id, WebsiteID: websiteByParentId[t.ParentId], t.Slug, Normalized: SlugGenerator.Normalize(t.Slug, maxLength)))
            .ToList();

        // A row that's already normalized keeps its exact value, which reserves that slot; a row
        // that needs fixing never reserves its OLD value (that's the value being abandoned) — doing
        // so would let one dirty row's stale slug wrongly evict another dirty row's freshly assigned
        // one when both started out identical (exactly the collision this pass exists to resolve).
        var reservedByWebsite = mainWork.Where(r => r.Normalized == r.Slug).Select(r => (r.WebsiteID, r.Slug))
            .Concat(translationWork.Where(r => r.Normalized == r.Slug).Select(r => (r.WebsiteID, r.Slug)))
            .GroupBy(x => x.WebsiteID)
            .ToDictionary(g => g.Key, g => new HashSet<string>(g.Select(x => x.Slug), StringComparer.OrdinalIgnoreCase));

        HashSet<string> ReservedFor(int websiteId)
        {
            if (!reservedByWebsite.TryGetValue(websiteId, out var set))
                reservedByWebsite[websiteId] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return set;
        }

        var changed = false;

        foreach (var row in mainWork.Where(r => r.Normalized != r.Slug))
        {
            var used = ReservedFor(row.WebsiteID);
            var unique = SlugGenerator.MakeUnique(row.Normalized, used);
            used.Add(unique);

            updateMain(row.Id, unique);
            changed = true;
        }

        foreach (var row in translationWork.Where(r => r.Normalized != r.Slug))
        {
            var used = ReservedFor(row.WebsiteID);
            var unique = SlugGenerator.MakeUnique(row.Normalized, used);
            used.Add(unique);

            updateTranslation(row.Id, unique);
            changed = true;
        }

        if (changed) await context.SaveChangesAsync(ct);
    }
}
