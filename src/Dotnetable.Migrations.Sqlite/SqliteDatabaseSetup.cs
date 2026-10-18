using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dotnetable.Migrations.Sqlite;

/// <summary>
/// SQLite provider wiring. Decimal storage lives here, not in the shared model:
/// the other providers keep their <c>decimal(p,s)</c> column types.
/// </summary>
public static class SqliteDatabaseSetup
{
    public const string MigrationsAssemblyName = "Dotnetable.Migrations.Sqlite";

    public static void Configure(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(MigrationsAssemblyName));
        ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(new SqliteDecimalOptionsExtension());
    }
}

/// <summary>
/// Forces every <see cref="decimal"/> column to SQLite <c>TEXT</c>.
/// SQLite has no decimal type. A <c>decimal(p,s)</c> declaration gets NUMERIC affinity and
/// can rewrite a value as a binary float. TEXT affinity keeps the exact decimal text that
/// Microsoft.Data.Sqlite already binds. This is a store type, not a string value converter,
/// so LINQ comparisons stay numeric.
/// Fluent <c>HasColumnType("decimal(...)")</c> is Explicit, so a normal convention cannot override it.
/// </summary>
internal sealed class SqliteDecimalOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services) =>
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConventionSetPlugin, SqliteDecimalConventionSetPlugin>());

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
        }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "SqliteDecimalText ";

        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) =>
            debugInfo["Sqlite:DecimalAsText"] = "1";
    }
}

internal sealed class SqliteDecimalConventionSetPlugin : IConventionSetPlugin
{
    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.ModelFinalizingConventions.Add(new SqliteDecimalConvention());
        return conventionSet;
    }
}

internal sealed class SqliteDecimalConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var property in modelBuilder.Metadata.GetEntityTypes().SelectMany(entity => entity.GetDeclaredProperties()))
        {
            var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
            if (clrType != typeof(decimal))
                continue;

            // Explicit overwrites HasColumnType("decimal(...)") from the shared AppDbContext.
#pragma warning disable EF1001
            ((ConventionAnnotatable)property).SetAnnotation(
                RelationalAnnotationNames.ColumnType,
                "TEXT",
                ConfigurationSource.Explicit);
#pragma warning restore EF1001
        }
    }
}
