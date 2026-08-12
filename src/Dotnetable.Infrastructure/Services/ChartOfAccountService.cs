using Dotnetable.Application.DTOs;
using Dotnetable.Application.Financial;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ChartOfAccountService : IChartOfAccountService
{
    private readonly AppDbContext _context;

    public ChartOfAccountService(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<ChartAccountDto>> GetTreeAsync(int websiteId, CancellationToken ct = default)
    {
        await EnsureSeededAsync(websiteId, ct);
        var flat = await GetFlatAsync(websiteId, ct);
        return BuildTree(flat, null);
    }

    public async Task<IReadOnlyList<ChartOfAccount>> GetFlatAsync(int websiteId, CancellationToken ct = default) =>
        await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Code)
            .ToListAsync(ct);

    public async Task EnsureSeededAsync(int websiteId, CancellationToken ct = default)
    {
        if (await _context.ChartOfAccounts.AnyAsync(a => a.WebsiteID == websiteId, ct))
            return;

        var website = await _context.Websites.AsNoTracking().FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);
        var currency = website?.DefaultCurrencyCode ?? "USD";

        var seed = new (string Code, string Name, GlAccountType Type, int Sort)[]
        {
            ("1100", "Cash", GlAccountType.Asset, 10),
            ("1200", "Bank", GlAccountType.Asset, 20),
            ("1300", "Accounts receivable", GlAccountType.Asset, 30),
            ("1400", "Inventory", GlAccountType.Asset, 40),
            ("2100", "Accounts payable", GlAccountType.Liability, 50),
            ("2200", "Tax payable", GlAccountType.Liability, 60),
            ("2300", "Insurance payable", GlAccountType.Liability, 70),
            ("2400", "Payroll payable", GlAccountType.Liability, 80),
            ("3100", "Equity", GlAccountType.Equity, 90),
            ("3200", "Retained earnings", GlAccountType.Equity, 95),
            ("4100", "Sales", GlAccountType.Income, 100),
            ("4200", "Shipping income", GlAccountType.Income, 110),
            ("4300", "Markup income", GlAccountType.Income, 120),
            ("4400", "Other income", GlAccountType.Income, 130),
            ("5100", "Cost of goods sold", GlAccountType.Expense, 140),
            ("5200", "Payroll expense", GlAccountType.Expense, 150),
            ("5300", "Employer insurance expense", GlAccountType.Expense, 160),
            ("5400", "Refunds & discounts", GlAccountType.Expense, 170),
            ("5500", "Vendor settlements", GlAccountType.Expense, 180),
            ("5600", "Operating expenses", GlAccountType.Expense, 190),
        };

        var byCode = new Dictionary<string, ChartOfAccount>(StringComparer.OrdinalIgnoreCase);
        foreach (var (code, name, type, sort) in seed)
        {
            var acc = new ChartOfAccount
            {
                WebsiteID = websiteId,
                Code = code,
                Name = name,
                AccountType = (byte)type,
                IsActive = true,
                IsSystem = true,
                SortOrder = sort,
            };
            _context.ChartOfAccounts.Add(acc);
            byCode[code] = acc;
        }
        await _context.SaveChangesAsync(ct);

        // Default L1 → L2 maps (debit, credit)
        void Map(string type, byte? flow, string debit, string credit)
        {
            _context.LedgerAccountMaps.Add(new LedgerAccountMap
            {
                WebsiteID = websiteId,
                TransactionType = type,
                Flow = flow,
                DebitAccountID = byCode[debit].ChartOfAccountID,
                CreditAccountID = byCode[credit].ChartOfAccountID,
                IsActive = true,
            });
        }

        Map(FinancialTransactionTypes.CustomerPayment, FinancialFlow.In, "1100", "4100");
        Map(FinancialTransactionTypes.AdditionalCharge, FinancialFlow.In, "1100", "4400");
        Map(FinancialTransactionTypes.CustomerRefund, FinancialFlow.Out, "5400", "1100");
        Map(FinancialTransactionTypes.OrderShipping, FinancialFlow.Component, "1300", "4200");
        Map(FinancialTransactionTypes.OrderMarkup, FinancialFlow.Component, "1300", "4300");
        Map(FinancialTransactionTypes.OrderLineRevenue, FinancialFlow.Component, "1300", "4100");
        Map(FinancialTransactionTypes.OrderLineCost, FinancialFlow.Component, "5100", "1400");
        Map(FinancialTransactionTypes.OrderLineProfit, FinancialFlow.Component, "1300", "4100");
        Map(FinancialTransactionTypes.OrderTax, FinancialFlow.Component, "1300", "2200");
        Map(FinancialTransactionTypes.OrderDiscount, FinancialFlow.Component, "5400", "1300");
        Map(FinancialTransactionTypes.VendorSettlement, FinancialFlow.Out, "5500", "2100");
        Map(FinancialTransactionTypes.SettlementPaid, FinancialFlow.Out, "2100", "1200");

        // Ensure current open fiscal period for the year
        var year = DateTime.UtcNow.Year;
        if (!await _context.FiscalPeriods.AnyAsync(p => p.WebsiteID == websiteId && p.PeriodFrom.Year == year, ct))
        {
            _context.FiscalPeriods.Add(new FiscalPeriod
            {
                WebsiteID = websiteId,
                Name = $"{year}",
                PeriodFrom = new DateOnly(year, 1, 1),
                PeriodTo = new DateOnly(year, 12, 31),
                IsClosed = false,
                CreatedAt = DateTime.UtcNow,
            });
        }

        await _context.SaveChangesAsync(ct);
        _ = currency; // reserved for multi-currency journals later
    }

    public async Task<ChartOfAccount> UpsertAsync(ChartOfAccount account, CancellationToken ct = default)
    {
        if (account.ChartOfAccountID == 0)
        {
            account.IsSystem = false;
            _context.ChartOfAccounts.Add(account);
        }
        else
        {
            var existing = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.ChartOfAccountID == account.ChartOfAccountID, ct)
                ?? throw new InvalidOperationException("Account not found.");
            if (existing.IsSystem)
            {
                existing.Name = account.Name;
                existing.IsActive = account.IsActive;
                existing.SortOrder = account.SortOrder;
            }
            else
            {
                existing.Code = account.Code;
                existing.Name = account.Name;
                existing.AccountType = account.AccountType;
                existing.ParentAccountID = account.ParentAccountID;
                existing.IsActive = account.IsActive;
                existing.SortOrder = account.SortOrder;
            }
            account = existing;
        }
        await _context.SaveChangesAsync(ct);
        return account;
    }

    public async Task SetActiveAsync(int accountId, bool active, CancellationToken ct = default)
    {
        var acc = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.ChartOfAccountID == accountId, ct);
        if (acc is null) return;
        acc.IsActive = active;
        await _context.SaveChangesAsync(ct);
    }

    private static List<ChartAccountDto> BuildTree(IReadOnlyList<ChartOfAccount> flat, int? parentId)
    {
        return flat.Where(a => a.ParentAccountID == parentId)
            .Select(a => new ChartAccountDto
            {
                ChartOfAccountID = a.ChartOfAccountID,
                ParentAccountID = a.ParentAccountID,
                Code = a.Code,
                Name = a.Name,
                AccountType = a.AccountType,
                IsActive = a.IsActive,
                IsSystem = a.IsSystem,
                SortOrder = a.SortOrder,
                Children = BuildTree(flat, a.ChartOfAccountID),
            }).ToList();
    }
}
