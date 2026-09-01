using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
// OrderStatus lives next to IOrderService

namespace Dotnetable.Infrastructure.Services;

public class TaxReportService : ITaxReportService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public TaxReportService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<VatReportDto> GetVatReportAsync(VatReportRequest request, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var website = await _context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteID == request.WebsiteId, ct)
            ?? throw new InvalidOperationException("Website not found.");

        var fromDt = request.From.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDt = request.To.ToDateTime(new TimeOnly(23, 59, 59), DateTimeKind.Utc);

        // Output VAT: customer-facing orders in range (exclude cancelled and orders not reported to tax).
        var orders = await _context.Orders.AsNoTracking()
            .Where(o => o.WebsiteID == request.WebsiteId
                        && o.CreatedAt >= fromDt && o.CreatedAt <= toDt
                        && o.Status != (byte)OrderStatus.Cancelled
                        && o.ReportToTax)
            .OrderBy(o => o.CreatedAt)
            .Select(o => new
            {
                o.OrderID,
                o.OrderNumber,
                o.CreatedAt,
                o.CurrencyCode,
                o.SubTotal,
                o.DiscountTotal,
                o.ShippingTotal,
                o.TaxTotal,
                o.GrandTotal,
                o.PricesIncludeTax,
                o.TaxBreakdownJson,
            })
            .ToListAsync(ct);

        var orderLines = orders.Select(o =>
        {
            var net = o.PricesIncludeTax
                ? Math.Max(0, o.GrandTotal - o.TaxTotal)
                : Math.Max(0, o.SubTotal - o.DiscountTotal + o.ShippingTotal);
            var tax = o.TaxTotal;
            var gross = o.GrandTotal;
            return new VatReportLineDto
            {
                Source = "Order",
                DocumentId = o.OrderID,
                DocumentNumber = o.OrderNumber,
                Date = o.CreatedAt,
                CurrencyCode = o.CurrencyCode,
                Net = net,
                Tax = tax,
                Gross = gross,
                PricesIncludeTax = o.PricesIncludeTax,
                Counterparty = null,
                BreakdownJson = o.TaxBreakdownJson,
            };
        }).ToList();

        // Settlement tax lines (B2B / inter-site) in period.
        var settlements = await _context.Settlements.AsNoTracking()
            .Include(s => s.Vendor)
            .Include(s => s.Supplier)
            .Include(s => s.TargetWebsite)
            .Where(s => s.WebsiteID == request.WebsiteId
                        && s.CreatedAt >= fromDt && s.CreatedAt <= toDt
                        && s.Status != (byte)SettlementStatus.Cancelled)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

        var settlementLines = settlements.Select(s =>
        {
            var net = s.NetAmount != 0 || s.TaxAmount != 0 ? s.NetAmount : s.TotalAmount;
            var tax = s.TaxAmount;
            var gross = s.TotalAmount != 0 ? s.TotalAmount : net + tax;
            var counterparty = s.Supplier?.LegalName ?? s.Supplier?.Name
                ?? s.Vendor?.Name
                ?? s.TargetWebsite?.BrandName
                ?? s.TargetWebsite?.TradeName;
            return new VatReportLineDto
            {
                Source = "Settlement",
                DocumentId = s.SettlementID,
                DocumentNumber = $"S-{s.SettlementID}",
                Date = s.CreatedAt,
                CurrencyCode = s.CurrencyCode,
                Net = net,
                Tax = tax,
                Gross = gross,
                PricesIncludeTax = false,
                Counterparty = counterparty,
                BreakdownJson = s.Note,
            };
        }).ToList();

        return new VatReportDto
        {
            WebsiteId = request.WebsiteId,
            From = request.From,
            To = request.To,
            SellerLegalName = website.SellerLegalName ?? website.TradeName,
            SellerTaxId = website.SellerTaxId,
            SellerVatNumber = website.SellerVatNumber,
            SellerEconomicCode = website.SellerEconomicCode,
            DefaultCurrencyCode = website.DefaultCurrencyCode,
            OutputSales = new VatBucketDto
            {
                Net = orderLines.Sum(x => x.Net),
                Tax = orderLines.Sum(x => x.Tax),
                Gross = orderLines.Sum(x => x.Gross),
                DocumentCount = orderLines.Count,
            },
            SettlementTax = new VatBucketDto
            {
                Net = settlementLines.Sum(x => x.Net),
                Tax = settlementLines.Sum(x => x.Tax),
                Gross = settlementLines.Sum(x => x.Gross),
                DocumentCount = settlementLines.Count,
            },
            OrderLines = orderLines,
            SettlementLines = settlementLines,
        };
    }

    public async Task<OrderInvoiceDto?> GetOrderInvoiceAsync(int orderId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var order = await _context.Orders.AsNoTracking()
            .Include(o => o.OrderItems).ThenInclude(i => i.ProductVariant)
            .Include(o => o.WebsiteClient)
            .Include(o => o.Website)
            .Include(o => o.ShippingMethod)
            .FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null) return null;

        var w = order.Website;
        var c = order.WebsiteClient;
        var codePrefix = Domain.ProductCode.NormalizePrefix(w.ProductCodePrefix);

        return new OrderInvoiceDto
        {
            OrderId = order.OrderID,
            WebsiteId = order.WebsiteID,
            OrderNumber = order.OrderNumber,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
            Status = order.Status,
            CurrencyCode = order.CurrencyCode,
            PreparationStatus = order.PreparationStatus,
            ShippingStatus = order.ShippingStatus,
            ShippingTrackingCode = order.ShippingTrackingCode,
            ShippedAt = order.ShippedAt,
            ShippingMethodID = order.ShippingMethodID,
            ShippingMethodName = order.ShippingMethod?.Title,
            Seller = new InvoicePartyDto
            {
                Name = w.BrandName,
                LegalName = w.SellerLegalName ?? w.TradeName,
                TaxId = w.SellerTaxId,
                EconomicCode = w.SellerEconomicCode,
                VatNumber = w.SellerVatNumber,
                RegistrationNumber = w.SellerRegistrationNumber,
                Email = w.Email,
                Phone = w.Mobile,
            },
            Buyer = new InvoicePartyDto
            {
                Name = string.Join(' ', new[] { c.Givenname, c.Surname }.Where(x => !string.IsNullOrWhiteSpace(x))),
                Email = c.Email,
                Phone = c.Cellphone,
                Address = order.AddressSnapshot,
            },
            Lines = order.OrderItems.Select(i => new InvoiceLineDto
            {
                Title = i.TitleSnapshot,
                ProductID = i.ProductVariant is { ProductID: > 0 } p ? p.ProductID : null,
                ProductVariantID = i.ProductVariantID,
                ProductCode = i.ProductVariant is { ProductID: > 0 } pv
                    ? Domain.ProductCode.Format(codePrefix, pv.ProductID)
                    : null,
                Sku = i.SkuSnapshot,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountAmount = i.DiscountAmount,
                LineTotal = i.TotalPrice,
            }).ToList(),
            SubTotal = order.SubTotal,
            DiscountTotal = order.DiscountTotal,
            ShippingTotal = order.ShippingTotal,
            TaxTotal = order.TaxTotal,
            GrandTotal = order.GrandTotal,
            MarkupTotal = order.MarkupTotal,
            PricesIncludeTax = order.PricesIncludeTax,
            ReportToTax = order.ReportToTax,
            SalesChannel = order.SalesChannel,
            TaxBreakdownJson = order.TaxBreakdownJson,
            AddressSnapshot = order.AddressSnapshot,
            Note = order.Note,
        };
    }
}
