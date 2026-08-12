using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public sealed class TaxReconciliationDto
{
    public decimal OrderTaxTotal { get; init; }
    public decimal SettlementTaxTotal { get; init; }
    public decimal LedgerTaxComponentTotal { get; init; }
    public decimal GlTaxPayableMovement { get; init; }
    public decimal DocumentTaxTotal => OrderTaxTotal + SettlementTaxTotal;
    public decimal DiffLedgerVsDocuments => LedgerTaxComponentTotal - DocumentTaxTotal;
    public decimal DiffGlVsDocuments => GlTaxPayableMovement - DocumentTaxTotal;
}

public interface IAccountingReportService
{
    Task<IReadOnlyList<TrialBalanceRowDto>> GetTrialBalanceAsync(
        int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default);

    Task<ProfitAndLossDto> GetProfitAndLossAsync(
        int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default);

    Task<TaxReconciliationDto> GetTaxReconciliationAsync(
        int websiteId, DateOnly from, DateOnly to, CancellationToken ct = default);

    Task<byte[]> ExportTrialBalanceExcelAsync(int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default);
    Task<byte[]> ExportProfitAndLossExcelAsync(int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default);
    Task<byte[]> ExportJournalsExcelAsync(int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default);
}

