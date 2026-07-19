using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Global bank master list (mirrors Currency: shared reference data, editable from the master website).</summary>
public interface IBankService
{
    Task<List<Bank>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<Bank>> GetPagedAsync(GridQuery query, CancellationToken ct = default);
    Task<Bank?> GetByIdAsync(int bankId, CancellationToken ct = default);
    Task<Bank> CreateAsync(Bank bank, CancellationToken ct = default);
    Task<bool> UpdateAsync(Bank bank, CancellationToken ct = default);
    Task<bool> DeleteAsync(int bankId, CancellationToken ct = default);
}
