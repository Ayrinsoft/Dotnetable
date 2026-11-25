using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class VatInvoiceService : IVatInvoiceService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public VatInvoiceService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<VatInvoiceResult> IssueFromOrderAsync(int orderId, int? memberId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            var existing = await context.VatInvoices.AsNoTracking()
                .Include(v => v.Lines)
                .Include(v => v.Order)
                .FirstOrDefaultAsync(v => v.OrderID == orderId && v.Direction == (byte)VatInvoiceDirection.Issued, ct);
            if (existing is not null)
                return new VatInvoiceResult(true, null, Map(existing));

            var order = await context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Website)
                .FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
            if (order is null)
                return new VatInvoiceResult(false, "Order not found.", null);
            if (order.Status == (byte)OrderStatus.Cancelled)
                return new VatInvoiceResult(false, "A cancelled order cannot be issued.", null);

            var website = order.Website;
            var invoice = new VatInvoice
            {
                WebsiteID = order.WebsiteID,
                Direction = (byte)VatInvoiceDirection.Issued,
                InvoiceNumber = order.OrderNumber,
                InvoiceDate = DateTime.UtcNow,
                OrderID = order.OrderID,
                CurrencyCode = order.CurrencyCode,
                OurName = Clip(First(website.SellerLegalName, website.TradeName, website.BrandName), 200),
                OurNationalId = Clip(website.SellerTaxId, 32),
                OurEconomicCode = Clip(website.SellerEconomicCode, 50),
                OurVatNumber = Clip(website.SellerVatNumber, 50),
                OurRegistrationNumber = Clip(website.SellerRegistrationNumber, 50),
                OurPhone = Clip(website.Mobile, 32),
                OurEmail = Clip(website.Email, 120),
                OtherName = Clip(order.BuyerName, 200),
                OtherNationalId = Clip(order.BuyerNationalId, 32),
                OtherPhone = Clip(order.BuyerPhone, 32),
                OtherEmail = Clip(order.BuyerEmail, 120),
                OtherAddress = Clip(order.AddressSnapshot, 500),
                NetTotal = order.PricesIncludeTax
                    ? Math.Max(0, order.GrandTotal - order.TaxTotal)
                    : Math.Max(0, order.SubTotal - order.DiscountTotal + order.ShippingTotal),
                DiscountTotal = order.DiscountTotal,
                TaxTotal = order.TaxTotal,
                PayableTotal = order.GrandTotal,
                PricesIncludeTax = order.PricesIncludeTax,
                Note = Clip(order.Note, 1000),
                CreatedByMemberID = memberId,
                CreatedAt = DateTime.UtcNow,
            };

            foreach (var item in order.OrderItems.OrderBy(i => i.OrderItemID))
            {
                var afterDiscount = Math.Max(0, item.TotalPrice - item.DiscountAmount);
                var tax = item.TaxAmount;
                var net = order.PricesIncludeTax ? Math.Max(0, afterDiscount - tax) : afterDiscount;
                var payable = order.PricesIncludeTax ? afterDiscount : afterDiscount + tax;
                var description = string.IsNullOrWhiteSpace(item.LineDescription) ? item.TitleSnapshot : item.TitleSnapshot + " — " + item.LineDescription;
                invoice.Lines.Add(new VatInvoiceLine
                {
                    Description = Clip(description, 1000) ?? item.TitleSnapshot,
                    Quantity = item.Quantity,
                    Unit = "عدد",
                    UnitPrice = item.UnitPrice,
                    DiscountAmount = item.DiscountAmount,
                    NetAmount = net,
                    TaxAmount = tax,
                    PayableAmount = payable,
                });
            }

            if (order.ShippingTotal > 0)
            {
                invoice.Lines.Add(new VatInvoiceLine
                {
                    Description = "حمل و نقل",
                    Quantity = 1,
                    Unit = "سرویس",
                    UnitPrice = order.ShippingTotal,
                    DiscountAmount = 0,
                    NetAmount = order.ShippingTotal,
                    TaxAmount = 0,
                    PayableAmount = order.ShippingTotal,
                });
            }

            context.VatInvoices.Add(invoice);
            await using var tx = await context.Database.BeginTransactionAsync(ct);
            try
            {
                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch (DbUpdateException)
            {
                return new VatInvoiceResult(false, "This order already has an issued invoice.", null);
            }

            invoice.Order = order;
            return new VatInvoiceResult(true, null, Map(invoice));
        });
    }

    public async Task<VatInvoiceResult> ReceiveAsync(VatInvoiceReceiveWrite write, int? memberId, CancellationToken ct = default)
    {
        var number = (write.InvoiceNumber ?? "").Trim();
        if (number.Length is < 1 or > 40)
            return new VatInvoiceResult(false, "Invoice number is required.", null);
        var supplier = (write.SupplierName ?? "").Trim();
        if (supplier.Length < 2)
            return new VatInvoiceResult(false, "Supplier name is required.", null);
        var nationalId = NationalIdOrNull(write.SupplierNationalId);
        if (!string.IsNullOrWhiteSpace(write.SupplierNationalId) && nationalId is null)
            return new VatInvoiceResult(false, "National id must be 4 to 32 letters or digits.", null);
        if (write.Lines.Count == 0)
            return new VatInvoiceResult(false, "At least one line is required.", null);

        var lines = new List<VatInvoiceLine>();
        foreach (var line in write.Lines)
        {
            var description = (line.Description ?? "").Trim();
            if (description.Length < 2)
                return new VatInvoiceResult(false, "Each line needs a description.", null);
            if (line.Quantity <= 0 || line.UnitPrice < 0 || line.DiscountAmount < 0 || line.TaxAmount < 0)
                return new VatInvoiceResult(false, "Quantity, price, discount, and tax must be zero or more.", null);
            var gross = Math.Round(line.Quantity * line.UnitPrice, 4, MidpointRounding.AwayFromZero);
            if (line.DiscountAmount > gross)
                return new VatInvoiceResult(false, "Discount cannot exceed the line amount.", null);
            var net = gross - line.DiscountAmount;
            lines.Add(new VatInvoiceLine
            {
                Description = Clip(description, 1000)!,
                Quantity = line.Quantity,
                Unit = Clip(string.IsNullOrWhiteSpace(line.Unit) ? "عدد" : line.Unit.Trim(), 40),
                UnitPrice = line.UnitPrice,
                DiscountAmount = line.DiscountAmount,
                NetAmount = net,
                TaxAmount = line.TaxAmount,
                PayableAmount = net + line.TaxAmount,
            });
        }

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var website = await context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteID == write.WebsiteId, ct);
        if (website is null)
            return new VatInvoiceResult(false, "Website not found.", null);

        var currency = string.IsNullOrWhiteSpace(write.CurrencyCode)
            ? website.DefaultCurrencyCode
            : write.CurrencyCode.Trim().ToUpperInvariant();
        if (currency.Length != 3)
            return new VatInvoiceResult(false, "Currency must be a 3-letter code.", null);

        var duplicate = await context.VatInvoices.AsNoTracking().AnyAsync(v =>
            v.WebsiteID == write.WebsiteId
            && v.Direction == (byte)VatInvoiceDirection.Received
            && v.InvoiceNumber == number, ct);
        if (duplicate)
            return new VatInvoiceResult(false, "This invoice number is already recorded.", null);

        var invoice = new VatInvoice
        {
            WebsiteID = write.WebsiteId,
            Direction = (byte)VatInvoiceDirection.Received,
            InvoiceNumber = number,
            InvoiceDate = DateTime.SpecifyKind(write.InvoiceDate, DateTimeKind.Utc),
            CurrencyCode = currency,
            OurName = Clip(First(website.SellerLegalName, website.TradeName, website.BrandName), 200),
            OurNationalId = Clip(website.SellerTaxId, 32),
            OurEconomicCode = Clip(website.SellerEconomicCode, 50),
            OurVatNumber = Clip(website.SellerVatNumber, 50),
            OurRegistrationNumber = Clip(website.SellerRegistrationNumber, 50),
            OurPhone = Clip(website.Mobile, 32),
            OurEmail = Clip(website.Email, 120),
            OtherName = Clip(supplier, 200),
            OtherNationalId = nationalId,
            OtherEconomicCode = Clip(write.SupplierEconomicCode, 50),
            OtherPhone = Clip(write.SupplierPhone, 32),
            OtherEmail = Clip(write.SupplierEmail, 120),
            OtherAddress = Clip(write.SupplierAddress, 500),
            OtherPostalCode = Clip(write.SupplierPostalCode, 20),
            NetTotal = lines.Sum(l => l.NetAmount),
            DiscountTotal = lines.Sum(l => l.DiscountAmount),
            TaxTotal = lines.Sum(l => l.TaxAmount),
            PayableTotal = lines.Sum(l => l.PayableAmount),
            PricesIncludeTax = false,
            Note = Clip(write.Note, 1000),
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
            Lines = lines,
        };
        context.VatInvoices.Add(invoice);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return new VatInvoiceResult(false, "This invoice number is already recorded.", null);
        }

        return new VatInvoiceResult(true, null, Map(invoice));
    }

    public async Task<VatInvoiceDto?> GetAsync(int vatInvoiceId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var invoice = await context.VatInvoices.AsNoTracking()
            .Include(v => v.Lines)
            .Include(v => v.Order)
            .FirstOrDefaultAsync(v => v.VatInvoiceID == vatInvoiceId, ct);
        return invoice is null ? null : Map(invoice);
    }

    public async Task<VatInvoiceSummaryDto?> FindIssuedForOrderAsync(int orderId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var invoice = await context.VatInvoices.AsNoTracking()
            .FirstOrDefaultAsync(v => v.OrderID == orderId && v.Direction == (byte)VatInvoiceDirection.Issued, ct);
        return invoice is null ? null : Summary(invoice);
    }

    public async Task<VatInvoiceDto?> GetIssuedForClientAsync(int orderId, int clientId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var owns = await context.Orders.AsNoTracking()
            .AnyAsync(o => o.OrderID == orderId && o.WebsiteClientID == clientId, ct);
        if (!owns)
            return null;
        var invoice = await context.VatInvoices.AsNoTracking()
            .Include(v => v.Lines)
            .Include(v => v.Order)
            .FirstOrDefaultAsync(v => v.OrderID == orderId && v.Direction == (byte)VatInvoiceDirection.Issued, ct);
        return invoice is null ? null : Map(invoice);
    }

    public async Task<IReadOnlyList<VatInvoiceSummaryDto>> ListAsync(VatInvoiceListQuery query, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var rows = context.VatInvoices.AsNoTracking().Where(v => v.WebsiteID == query.WebsiteId);
        if (DirectionOrNull(query.Direction) is byte direction)
            rows = rows.Where(v => v.Direction == direction);
        if (query.From is DateOnly from)
        {
            var start = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            rows = rows.Where(v => v.InvoiceDate >= start);
        }
        if (query.To is DateOnly to)
        {
            var end = to.ToDateTime(new TimeOnly(23, 59, 59), DateTimeKind.Utc);
            rows = rows.Where(v => v.InvoiceDate <= end);
        }

        var list = await rows.OrderByDescending(v => v.InvoiceDate).ThenByDescending(v => v.VatInvoiceID)
            .Take(200)
            .ToListAsync(ct);
        return list.Select(Summary).ToList();
    }

    internal static async Task<List<VatInvoice>> InPeriodAsync(
        AppDbContext context, int websiteId, byte direction, DateTime from, DateTime to, CancellationToken ct) =>
        await context.VatInvoices.AsNoTracking()
            .Where(v => v.WebsiteID == websiteId && v.Direction == direction && v.InvoiceDate >= from && v.InvoiceDate <= to)
            .OrderBy(v => v.InvoiceDate)
            .ToListAsync(ct);

    private static VatInvoiceSummaryDto Summary(VatInvoice invoice) => new()
    {
        VatInvoiceId = invoice.VatInvoiceID,
        Direction = DirectionName(invoice.Direction),
        InvoiceNumber = invoice.InvoiceNumber,
        InvoiceDate = invoice.InvoiceDate,
        OrderId = invoice.OrderID,
        OtherPartyName = invoice.OtherName ?? "",
        CurrencyCode = invoice.CurrencyCode,
        TaxTotal = invoice.TaxTotal,
        PayableTotal = invoice.PayableTotal,
    };

    private static VatInvoiceDto Map(VatInvoice invoice)
    {
        var ours = new InvoicePartyDto
        {
            Name = invoice.OurName ?? "",
            LegalName = invoice.OurName,
            TaxId = invoice.OurNationalId,
            NationalId = invoice.OurNationalId,
            EconomicCode = invoice.OurEconomicCode,
            VatNumber = invoice.OurVatNumber,
            RegistrationNumber = invoice.OurRegistrationNumber,
            Phone = invoice.OurPhone,
            Email = invoice.OurEmail,
            Address = invoice.OurAddress,
        };
        var other = new InvoicePartyDto
        {
            Name = invoice.OtherName ?? "",
            LegalName = invoice.OtherName,
            TaxId = invoice.OtherNationalId,
            NationalId = invoice.OtherNationalId,
            EconomicCode = invoice.OtherEconomicCode,
            Phone = invoice.OtherPhone,
            Email = invoice.OtherEmail,
            Address = JoinAddress(invoice.OtherAddress, invoice.OtherPostalCode),
        };
        var issued = invoice.Direction == (byte)VatInvoiceDirection.Issued;
        return new VatInvoiceDto
        {
            VatInvoiceId = invoice.VatInvoiceID,
            WebsiteId = invoice.WebsiteID,
            Direction = DirectionName(invoice.Direction),
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            OrderId = invoice.OrderID,
            OrderNumber = invoice.Order?.OrderNumber,
            CurrencyCode = invoice.CurrencyCode,
            Seller = issued ? ours : other,
            Buyer = issued ? other : ours,
            Lines = invoice.Lines
                .OrderBy(l => l.VatInvoiceLineID)
                .Select(l => new VatInvoiceLineDto
                {
                    Description = l.Description,
                    Quantity = l.Quantity,
                    Unit = l.Unit,
                    UnitPrice = l.UnitPrice,
                    DiscountAmount = l.DiscountAmount,
                    NetAmount = l.NetAmount,
                    TaxAmount = l.TaxAmount,
                    PayableAmount = l.PayableAmount,
                })
                .ToList(),
            NetTotal = invoice.NetTotal,
            DiscountTotal = invoice.DiscountTotal,
            TaxTotal = invoice.TaxTotal,
            PayableTotal = invoice.PayableTotal,
            PricesIncludeTax = invoice.PricesIncludeTax,
            Note = invoice.Note,
        };
    }

    private static string DirectionName(byte direction) =>
        direction == (byte)VatInvoiceDirection.Received ? "received" : "issued";

    private static byte? DirectionOrNull(string? direction)
    {
        if (string.Equals(direction, "issued", StringComparison.OrdinalIgnoreCase))
            return (byte)VatInvoiceDirection.Issued;
        if (string.Equals(direction, "received", StringComparison.OrdinalIgnoreCase))
            return (byte)VatInvoiceDirection.Received;
        return null;
    }

    private static string? JoinAddress(string? address, string? postal)
    {
        if (string.IsNullOrWhiteSpace(postal))
            return address;
        if (string.IsNullOrWhiteSpace(address))
            return postal;
        return address + " — " + postal;
    }

    private static string? First(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }

    private static string? Clip(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var text = value.Trim();
        return text.Length <= max ? text : text[..max];
    }

    private static string? NationalIdOrNull(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        var buffer = new System.Text.StringBuilder(raw.Length);
        foreach (var ch in raw.Trim())
        {
            if (ch is ' ' or '-' or '_' or '\u200c')
                continue;
            var next = ch;
            if (next is >= '\u06F0' and <= '\u06F9')
                next = (char)('0' + (next - '\u06F0'));
            else if (next is >= '\u0660' and <= '\u0669')
                next = (char)('0' + (next - '\u0660'));
            if (next is >= 'a' and <= 'z')
                next = char.ToUpperInvariant(next);
            if (next is (< '0' or > '9') and (< 'A' or > 'Z'))
                return null;
            buffer.Append(next);
        }

        var text = buffer.ToString();
        return text.Length is < 4 or > 32 ? null : text;
    }
}
