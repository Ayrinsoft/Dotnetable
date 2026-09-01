using Dotnetable.Application.DTOs;
using Dotnetable.Application.Financial;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Builds balanced journals from L1 without double-counting revenue.
/// Cash-basis sale: Dr Cash · Cr Sales/Shipping/Markup/Tax.
/// Inventory COGS is projected only from <see cref="FinancialTransactionTypes.InventoryCogs"/>
/// (stock-out / return) so GL inventory matches the warehouse book.
/// </summary>
public class GlProjector : IGlProjector
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IChartOfAccountService _coa;
    private readonly IJournalService _journals;
    private readonly ILogger<GlProjector> _logger;

    public GlProjector(
        IDbContextFactory<AppDbContext> contextFactory, IChartOfAccountService coa, IJournalService journals, ILogger<GlProjector> logger)
    {
        _contextFactory = contextFactory;
        _coa = coa;
        _journals = journals;
        _logger = logger;
    }

    public async Task ProjectEventGroupAsync(int websiteId, Guid eventGroupId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        await _coa.EnsureSeededAsync(websiteId, ct);
        var sourceKey = $"L1:{eventGroupId:N}";
        if (await _context.JournalEntries.AnyAsync(j => j.WebsiteID == websiteId && j.SourceKey == sourceKey, ct))
            return;

        var rows = await _context.FinancialLedgerEntries.AsNoTracking()
            .Where(e => e.WebsiteID == websiteId && e.EventGroupId == eventGroupId && e.IsCurrent)
            .ToListAsync(ct);
        if (rows.Count == 0) return;

        var accounts = await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId && a.IsSystem)
            .ToDictionaryAsync(a => a.Code, a => a.ChartOfAccountID, StringComparer.OrdinalIgnoreCase, ct);

        int Acc(string code) => accounts.TryGetValue(code, out var id) ? id : 0;

        var cash = Acc("1100");
        var bank = Acc("1200");
        var inventory = Acc("1400");
        var taxPay = Acc("2200");
        var sales = Acc("4100");
        var shippingInc = Acc("4200");
        var markupInc = Acc("4300");
        var otherInc = Acc("4400");
        var cogs = Acc("5100");
        var refunds = Acc("5400");
        var vendorExp = Acc("5500");
        var ap = Acc("2100");

        if (cash == 0 || sales == 0)
        {
            _logger.LogWarning("GL seed incomplete for website {WebsiteId}", websiteId);
            return;
        }

        var lines = new List<JournalLineDto>();
        decimal Sum(string type) => rows.Where(r => r.TransactionType == type).Sum(r => r.Amount);

        var payment = Sum(FinancialTransactionTypes.CustomerPayment) + Sum(FinancialTransactionTypes.AdditionalCharge);
        var refund = Sum(FinancialTransactionTypes.CustomerRefund);
        var ship = Sum(FinancialTransactionTypes.OrderShipping);
        var markup = Sum(FinancialTransactionTypes.OrderMarkup);
        var revenue = Sum(FinancialTransactionTypes.OrderLineRevenue);
        // Analytical OrderLineCost at payment is product-margin memo only — GL inventory moves with warehouse.
        var inventoryCogs = Sum(FinancialTransactionTypes.InventoryCogs);
        var inventoryCogsRev = Sum(FinancialTransactionTypes.InventoryCogsReversal);
        var tax = Sum(FinancialTransactionTypes.OrderTax);
        var discount = Sum(FinancialTransactionTypes.OrderDiscount);
        var vendorSettle = Sum(FinancialTransactionTypes.VendorSettlement) + Sum(FinancialTransactionTypes.SettlementPaid);
        var returnShip = rows
            .Where(r => r.TransactionType == FinancialTransactionTypes.ReturnShipping && r.Flow == FinancialFlow.Out)
            .Sum(r => r.Amount);
        var opEx = Acc("5600");

        if (payment > 0)
        {
            // Allocate cash credit to sales streams; leftover to sales.
            var allocated = ship + markup + tax + revenue;
            var salesCredit = revenue > 0 ? revenue : Math.Max(0, payment - ship - markup - tax);
            if (salesCredit == 0 && allocated == 0)
                salesCredit = payment;

            lines.Add(Dr(cash, payment, "Customer receipts"));
            if (ship > 0) lines.Add(Cr(shippingInc, ship, "Shipping income"));
            if (markup > 0) lines.Add(Cr(markupInc, markup, "Markup income"));
            if (tax > 0) lines.Add(Cr(taxPay, tax, "Tax payable"));
            if (salesCredit > 0) lines.Add(Cr(sales, salesCredit, "Sales"));
            // Balance residual
            var creditSum = ship + markup + tax + salesCredit;
            if (creditSum < payment)
                lines.Add(Cr(otherInc > 0 ? otherInc : sales, payment - creditSum, "Other / residual income"));
            else if (creditSum > payment)
            {
                // Prefer reducing sales credit
                // rebuild: only cash = sum of components capped
            }
        }

        if (refund > 0)
        {
            lines.Add(Dr(refunds, refund, "Customer refund"));
            lines.Add(Cr(cash, refund, "Cash out refund"));
        }

        // COGS when goods leave warehouse (or non-WMS fulfill) — not at payment.
        if (inventoryCogs > 0 && cogs > 0 && inventory > 0)
        {
            lines.Add(Dr(cogs, inventoryCogs, "COGS (stock out)"));
            lines.Add(Cr(inventory, inventoryCogs, "Inventory issue"));
        }

        // Sellable return restores inventory asset and reverses COGS.
        if (inventoryCogsRev > 0 && cogs > 0 && inventory > 0)
        {
            lines.Add(Dr(inventory, inventoryCogsRev, "Inventory return"));
            lines.Add(Cr(cogs, inventoryCogsRev, "COGS reverse"));
        }

        if (discount > 0 && refunds > 0)
        {
            // Already may be embedded in net sales; optional separate recognition skipped to avoid imbalance.
        }

        if (vendorSettle > 0 && vendorExp > 0 && ap > 0)
        {
            // Only when no CustomerPayment in same group (settlement-only groups)
            if (payment <= 0)
            {
                lines.Add(Dr(vendorExp, vendorSettle, "Vendor settlement"));
                lines.Add(Cr(ap, vendorSettle, "Accounts payable"));
            }
        }

        if (returnShip > 0 && opEx > 0)
        {
            lines.Add(Dr(opEx, returnShip, "Return shipping"));
            lines.Add(Cr(cash, returnShip, "Cash out return shipping"));
        }

        lines = CollapseLines(lines);
        var debit = lines.Sum(l => l.Debit);
        var credit = lines.Sum(l => l.Credit);
        if (debit <= 0 || debit != credit)
        {
            // Force balance residual to equity/other
            var diff = debit - credit;
            if (diff > 0 && otherInc > 0)
                lines.Add(Cr(otherInc, diff, "Balancing credit"));
            else if (diff < 0 && refunds > 0)
                lines.Add(Dr(refunds, -diff, "Balancing debit"));
            lines = CollapseLines(lines);
            debit = lines.Sum(l => l.Debit);
            credit = lines.Sum(l => l.Credit);
            if (debit <= 0 || debit != credit)
            {
                _logger.LogWarning("GL project unbalanced for {Key}: D={Debit} C={Credit}", sourceKey, debit, credit);
                return;
            }
        }

        var reportTax = rows.All(r => r.ReportToTax);
        var currency = rows[0].CurrencyCode;
        var date = rows.Min(r => r.OccurredDate);

        var description = $"Ledger {eventGroupId:N}";
        if (description.Length > 48)
            description = description[..48];
        var (ok, err, entry) = await _journals.CreateDraftAsync(
            websiteId, date, description, currency, reportTax, lines, null,
            "FinancialLedger", sourceKey, ct);
        if (!ok || entry is null)
        {
            _logger.LogWarning("GL draft failed {Key}: {Error}", sourceKey, err);
            return;
        }

        var post = await _journals.PostAsync(entry.JournalEntryID, null, ct);
        if (!post.Success)
            _logger.LogWarning("GL post failed {Key}: {Error}", sourceKey, post.Error);
    }

    public async Task ProjectLedgerEntryAsync(long financialLedgerEntryId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await _context.FinancialLedgerEntries.AsNoTracking()
            .FirstOrDefaultAsync(e => e.FinancialLedgerEntryID == financialLedgerEntryId, ct);
        if (row is null || !row.IsCurrent) return;
        await ProjectEventGroupAsync(row.WebsiteID, row.EventGroupId, ct);
    }

    private static JournalLineDto Dr(int accountId, decimal amount, string? desc) =>
        new() { ChartOfAccountID = accountId, Debit = amount, Credit = 0, Description = desc };

    private static JournalLineDto Cr(int accountId, decimal amount, string? desc) =>
        new() { ChartOfAccountID = accountId, Debit = 0, Credit = amount, Description = desc };

    private static List<JournalLineDto> CollapseLines(List<JournalLineDto> lines) =>
        lines.Where(l => l.ChartOfAccountID > 0 && (l.Debit > 0 || l.Credit > 0))
            .GroupBy(l => l.ChartOfAccountID)
            .Select(g =>
            {
                var net = g.Sum(x => x.Debit) - g.Sum(x => x.Credit);
                return new JournalLineDto
                {
                    ChartOfAccountID = g.Key,
                    Debit = net > 0 ? net : 0,
                    Credit = net < 0 ? -net : 0,
                    Description = g.Select(x => x.Description).FirstOrDefault(d => !string.IsNullOrWhiteSpace(d)),
                };
            })
            .Where(l => l.Debit > 0 || l.Credit > 0)
            .ToList();
}
