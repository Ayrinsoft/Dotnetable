using Dotnetable.Application.DTOs;

namespace Dotnetable.Infrastructure.Data;

/// <summary>
/// Seeds the initial dataset a brand-new installation needs: the master website, the full
/// permission catalogue, the default access levels (Administrators + Users) and the first
/// administrator member. Kept separate from <c>SetupService</c> so initial data lives in one place.
/// </summary>
public interface IInitialDataSeeder
{
    /// <summary>Inserts the first-run data into <paramref name="context"/> within a single transaction.</summary>
    Task SeedAsync(AppDbContext context, SetupRequest request, CancellationToken ct = default);
}
