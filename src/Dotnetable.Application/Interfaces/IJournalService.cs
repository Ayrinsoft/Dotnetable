using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IJournalService
{
    Task<PagedResult<JournalEntryDto>> GetPagedAsync(int websiteId, DateOnly? from, DateOnly? to, bool? posted, GridQuery query, CancellationToken ct = default);
    Task<JournalEntryDto?> GetByIdAsync(int journalEntryId, CancellationToken ct = default);

    Task<(bool Success, string? Error, JournalEntry? Entry)> CreateDraftAsync(
        int websiteId, DateOnly entryDate, string? description, string currencyCode,
        bool reportToTax, IReadOnlyList<JournalLineDto> lines, int? memberId,
        string? sourceType = null, string? sourceKey = null, CancellationToken ct = default);

    Task<(bool Success, string? Error)> PostAsync(int journalEntryId, int? memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error, JournalEntry? Reversal)> ReverseAsync(int journalEntryId, int? memberId, string? note, CancellationToken ct = default);
}
