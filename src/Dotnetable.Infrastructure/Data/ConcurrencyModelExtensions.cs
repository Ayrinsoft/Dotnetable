using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetable.Infrastructure.Data;

/// <summary>
/// Provider-aware optimistic-concurrency mapping for the three entities that carry a
/// <c>RowVersion</c> column (<c>InventoryItems</c>, <c>WarehouseStocks</c>, <c>ClientWallets</c>).
///
/// <para><c>IsRowVersion()</c> only does what its name promises on SQL Server, where
/// <c>rowversion</c> is a server-maintained counter. Emitted against MySQL it becomes a plain
/// <c>longblob</c> and against PostgreSQL a plain <c>bytea</c>: nothing ever changes the value, so
/// EF's <c>WHERE RowVersion = @old</c> predicate always matches and the "optimistic concurrency"
/// silently degraded to last-write-wins — two buyers could reserve the same unit, two debits could
/// spend the same wallet balance. Worse, the column is <c>NOT NULL</c> with nothing to generate it
/// off SQL Server, so inserts depend on the provider's default for an empty blob.</para>
///
/// <para>The fix has two halves. First, every counter that is actually contended now moves with an
/// atomic conditional <c>UPDATE … SET n = n + delta WHERE remaining >= delta</c> and treats the
/// affected-row count as the check — see <see cref="Services.InventoryService"/>,
/// <see cref="Services.VendorProductService"/>, <see cref="Services.WarehouseService"/> and
/// <see cref="Services.ClientWalletService"/>. A single UPDATE statement holds its row lock on
/// SQL Server, MySQL/InnoDB and PostgreSQL alike, so that half is correct everywhere and does not
/// depend on a token at all. Second, the token itself is only declared where the engine can
/// maintain it: native <c>rowversion</c> on SQL Server, and nothing on MySQL/PostgreSQL, where the
/// column stays an inert optional payload. Declaring a token that cannot work is worse than
/// declaring none — it reads as protection in code review while providing none at runtime.</para>
/// </summary>
internal static class ConcurrencyModelExtensions
{
    public static bool IsSqlServer(string? providerName) =>
        providerName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsPostgreSql(string? providerName) =>
        providerName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsMySql(string? providerName) =>
        providerName?.Contains("MySql", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>True when <paramref name="providerName"/> can maintain a real row-version token.</summary>
    public static bool SupportsRowVersionToken(string? providerName) => IsSqlServer(providerName);

    /// <summary>
    /// Maps <paramref name="rowVersion"/> as the entity's concurrency token on providers that can
    /// maintain one, and as an inert optional column everywhere else.
    /// </summary>
    public static EntityTypeBuilder<TEntity> ConfigureRowVersion<TEntity>(
        this EntityTypeBuilder<TEntity> entity,
        Expression<Func<TEntity, byte[]>> rowVersion,
        string? providerName)
        where TEntity : class
    {
        if (SupportsRowVersionToken(providerName))
        {
            entity.Property(rowVersion).IsRowVersion().IsConcurrencyToken();
            return entity;
        }

        // Kept so the entity, the SSDT scripts and all three InitialCreate migrations describe the
        // same table, but optional and never written: correctness comes from the atomic UPDATEs.
        entity.Property(rowVersion)
            .IsConcurrencyToken(false)
            .ValueGeneratedNever()
            .IsRequired(false)
            .HasMaxLength(8);

        return entity;
    }
}
