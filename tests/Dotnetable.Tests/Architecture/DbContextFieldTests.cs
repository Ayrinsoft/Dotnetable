using System.Reflection;
using Dotnetable.Infrastructure.Data;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Architecture;

/// <summary>
/// Guards the rule in AGENTS.md: an application service must not hold an <see cref="AppDbContext"/>
/// as a field.
///
/// <para>Why it matters: Blazor Server runs a layout, a page and several child components'
/// <c>OnInitializedAsync</c> concurrently inside one circuit scope. A scoped service that captured a
/// context in a field would share it across all of them and throw "A second operation was started on
/// this context instance". <see cref="DbContextConcurrencyTests"/> keeps <c>AppDbContext</c>
/// registered as transient, which stops services from sharing <em>each other's</em> context — but a
/// single service instance whose own two methods run concurrently still shares one, and that context
/// then lives for the whole circuit, accumulating tracked entities and serving stale reads.</para>
///
/// <para>The fix is <c>IDbContextFactory&lt;AppDbContext&gt;</c> and a short-lived context per call.
/// Sixty-odd services predate the rule, so this test carries them as a named baseline rather than
/// failing the build on day one: the list may shrink, never grow. A new service that takes a context
/// field fails here with an explanation, which is the point — the debt is fenced off, and it is
/// visible instead of implied.</para>
/// </summary>
public class DbContextFieldTests
{
    /// <summary>
    /// What is left of the pre-rule code. Every entry is a type still to be converted to
    /// <c>IDbContextFactory</c>. Delete names as they are converted; never add one.
    ///
    /// <para><c>GenericRepository</c> and <c>UnitOfWork</c> are registered in DI but resolved by
    /// nothing, so they cannot cause the circuit race today. They stay listed rather than deleted
    /// because the moment someone starts using them the problem is back.</para>
    /// </summary>
    private static readonly HashSet<string> Baseline = new(StringComparer.Ordinal)
    {
        "GenericRepository`1",
        "UnitOfWork",
    };

    [Fact]
    public void No_New_Service_May_Hold_An_AppDbContext_Field()
    {
        var offenders = typeof(AppDbContext).Assembly
            .GetTypes()
            .Where(IsCandidateService)
            .Where(HoldsDbContextField)
            .Select(type => type.Name)
            .Where(name => !Baseline.Contains(name))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        offenders.Should().BeEmpty(
            "a service holding an AppDbContext field shares one context across concurrent calls on a " +
            "Blazor circuit and keeps it alive for the life of that circuit. Inject " +
            "IDbContextFactory<AppDbContext> and open a short-lived context per call instead " +
            "(see DbContextFactoryExtensions). Offenders: {0}",
            string.Join(", ", offenders));
    }

    /// <summary>
    /// The baseline must only ever shrink. A name left here after its service was converted would
    /// silently re-permit the pattern for that type, so a stale entry fails too.
    /// </summary>
    [Fact]
    public void Baseline_Contains_No_Stale_Entries()
    {
        var stillOffending = typeof(AppDbContext).Assembly
            .GetTypes()
            .Where(IsCandidateService)
            .Where(HoldsDbContextField)
            .Select(type => type.Name)
            .ToHashSet(StringComparer.Ordinal);

        var stale = Baseline.Where(name => !stillOffending.Contains(name))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        stale.Should().BeEmpty(
            "these services no longer hold an AppDbContext field, so they must be removed from the " +
            "baseline — leaving them would let the pattern come back unnoticed. Stale: {0}",
            string.Join(", ", stale));
    }

    /// <summary>
    /// Only real, hand-written classes count. An <c>async</c> method that keeps a context in a local
    /// compiles to a state-machine struct/class with a field of that type, and lambdas compile to
    /// closure classes — both are correct short-lived usage and neither is what this rule is about.
    /// </summary>
    private static bool IsCandidateService(Type type) =>
        type is { IsClass: true, IsAbstract: false }
        && !type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false)
        && !type.Name.Contains('<', StringComparison.Ordinal)
        && !type.IsNested;

    private static bool HoldsDbContextField(Type type) =>
        type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Any(field => typeof(AppDbContext).IsAssignableFrom(field.FieldType));
}
