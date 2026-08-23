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

    [Fact]
    public async Task RecordAttachmentService_AllowsParallelList_OnSameInstance()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var factory = new TestDbContextFactory(options);
        var svc = new RecordAttachmentService(factory);

        var a = svc.ListAsync("Order", 1);
        var b = svc.ListAsync("Payment", 2);
        await Task.WhenAll(a, b);

        (await a).Should().BeEmpty();
        (await b).Should().BeEmpty();
    }

    /// <summary>
    /// Application services must not hold a long-lived <see cref="AppDbContext"/>.
    /// Blazor Server runs multiple child components (e.g. several attachment panels on an order)
    /// concurrently on one scoped service instance — a field-held context races.
    /// Use <see cref="IDbContextFactory{TContext}"/> + <see cref="DbContextFactoryExtensions"/>.
    /// </summary>
    [Fact]
    public void Services_Must_Not_Hold_AppDbContext_Field()
    {
        var asm = typeof(WebsiteService).Assembly;
        var offenders = asm.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true }
                        && t.Namespace == "Dotnetable.Infrastructure.Services"
                        && !t.Name.Contains('<'))
            .Where(t => t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                                    | BindingFlags.DeclaredOnly)
                .Any(f => f.FieldType == typeof(AppDbContext)))
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToList();

        const int baselineMax = 71; // shrink only; never raise. New services must use IDbContextFactory.

        offenders.Should().NotContain("RecordAttachmentService");
        offenders.Count.Should().BeLessThanOrEqualTo(baselineMax,
            "Do not add services that hold AppDbContext. Inject IDbContextFactory and UseAsync / " +
            $"UseAmbientOrCreateAsync. Offenders ({offenders.Count}): {string.Join(", ", offenders)}");
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
            typeof(RecordAttachmentService),
            typeof(StaffTaskService),
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
