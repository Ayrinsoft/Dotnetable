using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

public sealed class FiscalCalendarSettingsDto
{
    public FiscalPeriodCadence Cadence { get; init; } = FiscalPeriodCadence.Monthly;
    public byte FiscalYearStartMonth { get; init; } = 1;
    public byte FiscalWeekStartDay { get; init; } = 1;
    public int CloseDueDays { get; init; } = 5;
}

public interface IFiscalPeriodService
{
    Task<IReadOnlyList<FiscalPeriod>> GetAllAsync(int websiteId, bool? closedOnly = null, CancellationToken ct = default);
    Task<FiscalPeriod?> GetCurrentOpenAsync(int websiteId, DateOnly? asOf = null, CancellationToken ct = default);
    Task<FiscalCalendarSettingsDto> GetCalendarSettingsAsync(int websiteId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> SaveCalendarSettingsAsync(int websiteId, FiscalCalendarSettingsDto settings, CancellationToken ct = default);

    /// <summary>Generate missing periods for a calendar year using site cadence settings.</summary>
    Task<(bool Success, string? Error, int Created)> GenerateForYearAsync(int websiteId, int year, CancellationToken ct = default);

    Task<FiscalPeriod> EnsureYearAsync(int websiteId, int year, CancellationToken ct = default);

    /// <summary>
    /// Close period: block new journals, roll P&amp;L into retained earnings, post opening balances into the next period.
    /// </summary>
    Task<(bool Success, string? Error)> CloseAsync(int fiscalPeriodId, int? memberId, CancellationToken ct = default);

    /// <summary>Reopen only if it is the latest closed period and no later period has activity (admin recovery).</summary>
    Task<(bool Success, string? Error)> ReopenAsync(int fiscalPeriodId, int? memberId, CancellationToken ct = default);
}
