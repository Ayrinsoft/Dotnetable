using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

public class InitialDataSeederTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly InitialDataSeeder _seeder;

    public InitialDataSeederTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            // The seeder wraps its work in a transaction; the in-memory provider ignores
            // transactions, so silence the warning it would otherwise raise as an error.
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new AppDbContext(opts);
        // Services open a context per call now, so they get a factory over the same options;
        // the fixture keeps its own _context for seeding and asserting.
        var factory = new TestDbContextFactory(opts);

        var hasher = new Mock<IPasswordHasher<Member>>();
        hasher.Setup(h => h.HashPassword(It.IsAny<Member>(), It.IsAny<string>()))
            .Returns<Member, string>((_, pw) => $"HASHED:{pw}");

        _seeder = new InitialDataSeeder(hasher.Object);
    }

    private static SetupRequest NewRequest() => new()
    {
        TradeName = "Acme", BrandName = "Acme", WebsiteAddress = "acme.com",
        Manager = "Mgr", Mobile = "123", WebsiteEmail = "site@acme.com", DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
        Username = "admin", Password = "secret", Email = "admin@acme.com",
        Givenname = "Ada", Surname = "Min",
    };

    [Fact]
    public async Task SeedAsync_CreatesMasterWebsiteWithId1()
    {
        await _seeder.SeedAsync(_context, NewRequest());

        var website = await _context.Websites.SingleAsync();
        website.WebsiteID.Should().Be(1);
        website.AuthCode.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task SeedAsync_SeedsEveryRoleFromCatalog()
    {
        await _seeder.SeedAsync(_context, NewRequest());

        var keys = await _context.Roles.Select(r => r.RoleKey).ToListAsync();
        keys.Should().BeEquivalentTo(RoleCatalog.All.Select(r => r.Key));
    }

    [Fact]
    public async Task SeedAsync_AdministratorsPolicy_GrantsEveryRole()
    {
        await _seeder.SeedAsync(_context, NewRequest());

        var admin = await _context.Policies.SingleAsync(p => p.Title == DefaultPolicies.Administrators);
        var grantedRoleIds = await _context.PolicyRoles
            .Where(pr => pr.PolicyID == admin.PolicyID).Select(pr => pr.RoleID).ToListAsync();

        grantedRoleIds.Should().HaveCount(RoleCatalog.All.Count);
    }

    [Fact]
    public async Task SeedAsync_UsersPolicy_GrantsOnlyClientRoles()
    {
        await _seeder.SeedAsync(_context, NewRequest());

        var users = await _context.Policies.SingleAsync(p => p.Title == DefaultPolicies.Users);
        var grantedKeys = await _context.PolicyRoles
            .Where(pr => pr.PolicyID == users.PolicyID)
            .Join(_context.Roles, pr => pr.RoleID, r => r.RoleID, (_, r) => r.RoleKey)
            .ToListAsync();

        grantedKeys.Should().BeEquivalentTo(RoleCatalog.ClientKeys);
    }

    [Fact]
    public async Task SeedAsync_StaffPolicies_IncludeWarehouseSalesFinanceHr()
    {
        await _seeder.SeedAsync(_context, NewRequest());

        foreach (var (title, keys) in DefaultPolicies.StaffTemplates)
        {
            var policy = await _context.Policies.SingleAsync(p => p.Title == title);
            var granted = await _context.PolicyRoles
                .Where(pr => pr.PolicyID == policy.PolicyID)
                .Join(_context.Roles, pr => pr.RoleID, r => r.RoleID, (_, r) => r.RoleKey)
                .ToListAsync();
            granted.Should().BeEquivalentTo(keys, because: $"policy '{title}' should grant its template roles");
        }
    }

    [Fact]
    public async Task SeedAsync_CreatesAdminMemberBoundToAdministratorsPolicy()
    {
        await _seeder.SeedAsync(_context, NewRequest());

        var admin = await _context.Policies.SingleAsync(p => p.Title == DefaultPolicies.Administrators);
        var member = await _context.Members.SingleAsync();
        member.Username.Should().Be("admin");
        member.PolicyID.Should().Be(admin.PolicyID);
        member.Password.Should().Be("HASHED:secret");
    }

    [Fact]
    public async Task SeedAsync_SeedsAdminCatalogAndWebsiteDefaultLanguageSeparately()
    {
        await _seeder.SeedAsync(_context, NewRequest());

        var admin = await _context.Languages.Where(l => l.WebsiteID == null).ToListAsync();
        admin.Should().HaveCount(7);
        admin.Select(l => l.LanguageCode).Should().Contain("en");
        admin.Single(l => l.LanguageCode == "en").IsDefault.Should().BeTrue();

        var site = await _context.Languages.Where(l => l.WebsiteID != null).ToListAsync();
        site.Should().ContainSingle();
        site[0].LanguageCode.Should().Be("en");
        site[0].WebsiteID.Should().Be(1);
        site[0].IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_DoesNotDuplicateAdminLanguageWhenCatalogAlreadyPresent()
    {
        _context.Languages.Add(new Language
        {
            WebsiteID = null,
            LanguageCode = "en",
            LanguageCodeISO = "en-US",
            Name = "English",
            Priority = 0,
            Active = true,
            IsDefault = true,
            RTLDesign = false,
        });
        await _context.SaveChangesAsync();

        await _seeder.SeedAsync(_context, NewRequest());

        (await _context.Languages.CountAsync(l => l.WebsiteID == null && l.LanguageCode == "en"))
            .Should().Be(1);
        (await _context.Languages.CountAsync(l => l.WebsiteID == 1 && l.LanguageCode == "en"))
            .Should().Be(1);
    }

    [Fact]
    public async Task SeedAsync_WebsiteFeatures_HaveCreatedAtSet()
    {
        var before = DateTime.UtcNow.AddSeconds(-2);

        await _seeder.SeedAsync(_context, NewRequest());

        var features = await _context.WebsiteFeatures.ToListAsync();
        features.Should().NotBeEmpty();
        features.Should().OnlyContain(f => f.CreatedAt >= before && f.CreatedAt <= DateTime.UtcNow.AddSeconds(2));
        features.Should().OnlyContain(f => f.CreatedAt != default);
    }

    public void Dispose() => _context.Dispose();
}
