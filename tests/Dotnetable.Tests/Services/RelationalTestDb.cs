using Dotnetable.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Tests.Services;

/// <summary>
/// An isolated, throwaway relational database for tests that exercise code the EF InMemory provider
/// cannot run.
///
/// <para>InMemory is not a database: it has no SQL, so <c>ExecuteUpdate</c> / <c>ExecuteDelete</c>
/// throw outright. That matters here because the stock-reservation and wallet-balance paths were
/// deliberately rewritten as single conditional <c>UPDATE</c> statements — the whole point being that
/// the check and the write happen atomically in the database — and a fake that cannot execute SQL
/// cannot test them. SQLite in memory is a real relational engine and runs those statements.</para>
///
/// <para>SQLite keeps an in-memory database alive only while a connection to it is open, so the
/// connection is owned here and disposed with the fixture.</para>
/// </summary>
public sealed class RelationalTestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public RelationalTestDb()
    {
        // Foreign keys off. These are unit tests of service logic and they were written against the
        // InMemory provider, which enforces no relational constraints — so their fixtures seed only
        // the rows each test actually reads. Turning enforcement on here would fail them on missing
        // reference data rather than on the behaviour under test; the schema itself is covered by the
        // migrations and the SSDT project.
        _connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = ":memory:",
            Mode = SqliteOpenMode.Memory,
            ForeignKeys = false,
        }.ToString());
        _connection.Open();

        Options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDbContext(Options);
        context.Database.EnsureCreated();
    }

    public DbContextOptions<AppDbContext> Options { get; }

    /// <summary>A fresh context over the same database. Callers own the returned instance.</summary>
    public AppDbContext NewContext() => new(Options);

    public void Dispose() => _connection.Dispose();
}
