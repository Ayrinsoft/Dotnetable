using System.Reflection;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Dotnetable.Infrastructure.Services;
using Dotnetable.Tests.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dotnetable.Tests.Architecture;

/// <summary>
/// Guards against the Blazor Server + shared DbContext race that caused
/// "A second operation was started on this context instance...".
/// </summary>
public class DbContextConcurrencyTests
{
    [Fact]
    public void AppDbContext_IsRegisteredAsTransient_NotScoped()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = "SqlServer",
                ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=unused;Trusted_Connection=True;",
            })
            .Build();

        services.AddInfrastructure(config, Path.GetTempPath());

        var descriptors = services.Where(d => d.ServiceType == typeof(AppDbContext)).ToList();
        descriptors.Should().NotBeEmpty();
        descriptors.Should().OnlyContain(d => d.Lifetime == ServiceLifetime.Transient,
            "a scoped AppDbContext is shared by every service in a Blazor circuit and races when " +
            "layout/page/nav initialize in parallel. Keep it Transient (or inject IDbContextFactory).");
    }

    [Fact]
    public async Task TransientContexts_AllowParallelQueries_AcrossServiceInstances()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var factory = new TestDbContextFactory(options);

        // Seed once
        await using (var seed = factory.CreateDbContext())
        {
            seed.Websites.Add(new Domain.Entities.Website
            {
                TradeName = "A",
                BrandName = "A",
                WebsiteAddress = "a.test",
                AuthCode = Guid.NewGuid(),
                Active = true,
                Manager = "m",
                Mobile = "1",
                Email = "a@a.test",
                RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DefaultLanguageCode = "en",
                DefaultCurrencyCode = "USD",
            });
            await seed.SaveChangesAsync();
        }

        // Two service instances, each with its own context (mirrors Transient injection into scoped services)
        var left = new WebsiteService(factory);
        var right = new WebsiteService(factory);

        var t1 = left.GetAllAsync();
        var t2 = right.GetPagedAsync(new Application.DTOs.GridQuery());
        await Task.WhenAll(t1, t2);

        (await t1).Should().NotBeEmpty();
        (await t2).TotalCount.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Inventory of application services that still inject <see cref="AppDbContext"/> directly.
    /// Prefer <see cref="IDbContextFactory{TContext}"/> for new code (short-lived contexts).
    /// This test fails if the count grows without an intentional review — update the baseline
    /// only when you knowingly add another field-based context consumer.
    /// </summary>
    [Fact]
    public void Services_Injecting_AppDbContext_Field_Stay_Within_Baseline()
    {
        // Shrink this ceiling as services migrate to IDbContextFactory-only.
        const int baselineMax = 65;

        var asm = typeof(WebsiteService).Assembly;
        var offenders = asm.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true }
                        && t.Namespace == "Dotnetable.Infrastructure.Services"
                        && !t.Name.Contains('<')) // skip compiler-generated types
            .Where(t => t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                                    | BindingFlags.DeclaredOnly)
                .Any(f => f.FieldType == typeof(AppDbContext)))
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToList();

        offenders.Count.Should().BeLessThanOrEqualTo(baselineMax,
            "New services should inject IDbContextFactory<AppDbContext> and use short-lived contexts " +
            $"(DbContextFactoryExtensions). Offenders ({offenders.Count}): {string.Join(", ", offenders)}");
    }

    [Fact]
    public void NewServices_ShouldPrefer_IDbContextFactory_WhenBothNotNeeded()
    {
        // Canonical "safe" services already on the factory-only pattern.
        var factoryOnly = new[]
        {
            typeof(WebsiteService),
            typeof(AdminTaskService),
            typeof(AdminNotificationService),
            typeof(DatabaseUpdateService),
            typeof(SetupService),
        };

        foreach (var type in factoryOnly)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            fields.Should().NotContain(f => f.FieldType == typeof(AppDbContext),
                $"{type.Name} should not hold a long-lived AppDbContext field");
            fields.Should().Contain(f => f.FieldType == typeof(IDbContextFactory<AppDbContext>),
                $"{type.Name} should inject IDbContextFactory<AppDbContext>");
        }
    }
}
