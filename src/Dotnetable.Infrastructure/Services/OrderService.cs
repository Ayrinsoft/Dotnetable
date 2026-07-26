using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventory;
    private readonly IShippingService _shipping;
    private readonly ITaxService _tax;
    private readonly ICouponService _coupons;
    private readonly ICurrencyConversionService _currency;
    private readonly ICartService _cart;
    private readonly IAdminNotificationService _notifications;
    private readonly IVendorCreditService _vendorCredit;

    public OrderService(
        AppDbContext context, IInventoryService inventory, IShippingService shipping,
        ITaxService tax, ICouponService coupons, ICurrencyConversionService currency, ICartService cart,
        IAdminNotificationService notifications, IVendorCreditService vendorCredit)
    {
        _context = context;
        _inventory = inventory;
        _shipping = shipping;
        _tax = tax;
        _coupons = coupons;
        _currency = currency;
        _cart = cart;
        _notifications = notifications;
        _vendorCredit = vendorCredit;
    }

    public async Task<CheckoutResult> CheckoutAsync(
        int websiteId, int clientId, int cartId, int addressId, int shippingMethodId,
        string? currencyCode = null, string? note = null, CancellationToken ct = default)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product)
            .Include(c => c.CartItems).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.InventoryItems)
            .Include(c => c.CartItems).ThenInclude(i => i.VendorProduct).ThenInclude(vp => vp!.Vendor)
            .FirstOrDefaultAsync(c => c.CartID == cartId && c.WebsiteClientID == clientId, ct);
        if (cart is null || cart.CartItems.Count == 0)
            return new CheckoutResult(false, "Cart is empty.", null, null);

        var address = await _context.WebsiteClientAddresses
            .Include(a => a.City)
            .FirstOrDefaultAsync(a => a.WebsiteClientAddressID == addressId && a.WebsiteClientID == clientId, ct);
        if (address is null)
            return new CheckoutResult(false, "Address not found.", null, null);
        var stateId = address.City?.StateID;

        // Preload vendor credit needs for site-linked lines (USD bridge for credit accounting).
        var unitUsdByItem = new Dictionary<int, decimal>();
        foreach (var i in cart.CartItems)
            unitUsdByItem[i.CartItemID] = await ResolveUnitPriceUsdAsync(i, websiteId, ct);

        decimal creditNeedByVendor(int vendorId) => cart.CartItems
            .Where(i => i.VendorProduct?.VendorID == vendorId)
            .Sum(i => unitUsdByItem[i.CartItemID] * i.Quantity);

        // Stock / vendor listing re-validation.
        foreach (var item in cart.CartItems)
        {
            if (item.VendorProduct is { } vp)
            {
                if (!vp.IsActive || vp.Vendor is null || !vp.Vendor.IsActive)
                    return new CheckoutResult(false, $"Vendor listing unavailable for {item.ProductVariant.Product.Title}.", null, null);

                if (vp.Vendor.VendorType == (byte)VendorType.Site)
                {
                    // Only own products of linked website may be sold (no re-share).
                    if (vp.Vendor.LinkedWebsiteID is int lid && item.ProductVariant.Product.WebsiteID != lid)
                        return new CheckoutResult(false, $"Product {item.ProductVariant.Product.Title} cannot be sold via this vendor link.", null, null);

                    var availableCredit = vp.Vendor.AvailableCredit != 0 || vp.Vendor.AvailableCreditUsd == 0
                        ? vp.Vendor.AvailableCredit
                        : vp.Vendor.AvailableCreditUsd;
                    // creditNeed is USD; convert available when dual is populated.
                    var needUsd = creditNeedByVendor(vp.VendorID);
                    if (vp.Vendor.SettlementMode == 1 && vp.Vendor.AvailableCreditUsd < needUsd && availableCredit < needUsd)
                        return new CheckoutResult(false, $"Insufficient inter-site credit for vendor {vp.Vendor.Name}.", null, null);

                    if (vp.StockQuantity < item.Quantity)
                        return new CheckoutResult(false, $"Insufficient vendor stock for {item.ProductVariant.Product.Title}.", null, null);
                }
                else if (vp.StockQuantity < item.Quantity)
                {
                    return new CheckoutResult(false, $"Insufficient vendor stock for {item.ProductVariant.Product.Title}.", null, null);
                }
            }
            else
            {
                var availability = await _inventory.GetAvailabilityAsync(websiteId, item.ProductVariantID, ct);
                if (availability.Available < item.Quantity)
                    return new CheckoutResult(false, $"Insufficient stock for {item.ProductVariant.Product.Title}.", null, null);
            }
        }

        var totalWeight = cart.CartItems.Sum(i => (i.ProductVariant.Weight ?? 0) * i.Quantity);
        var shippingOptions = await _shipping.GetAvailableWithPricesAsync(websiteId, address.CountryId, stateId, address.CityId, totalWeight, ct);
        var shippingOption = shippingOptions.FirstOrDefault(o => o.Method.ShippingMethodID == shippingMethodId);
        if (shippingOption.Method is null)
            return new CheckoutResult(false, "Selected shipping method is not available for this address.", null, null);

        var subtotalUsd = cart.CartItems.Sum(i => unitUsdByItem[i.CartItemID] * i.Quantity);
        var taxUsd = await _tax.ComputeTaxAsync(websiteId, address.CountryId, stateId, subtotalUsd, ct);
        var shippingUsd = shippingOption.PriceUsd;

        decimal discountUsd = 0;
        Coupon? coupon = null;
        if (cart.CouponID is int couponId)
        {
            coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.CouponID == couponId, ct);
            if (coupon is not null)
            {
                var (valid, _, computed) = await _coupons.ValidateAndComputeAsync(websiteId, coupon.Code, clientId, subtotalUsd, ct);
                if (valid) discountUsd = computed;
                else coupon = null;
            }
        }

        var grandTotalUsd = subtotalUsd + shippingUsd + taxUsd - discountUsd;
        var (resolvedCurrency, rate) = await _currency.GetActiveRateAsync(websiteId, currencyCode, ct);

        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        var order = new Order
        {
            WebsiteID = websiteId,
            OrderNumber = GenerateOrderNumber(websiteId),
            WebsiteClientID = clientId,
            Status = (byte)OrderStatus.PendingPayment,
            CurrencyCode = resolvedCurrency,
            ExchangeRateToUsd = rate,
            SubTotal = subtotalUsd * rate,
            DiscountTotal = discountUsd * rate,
            ShippingTotal = shippingUsd * rate,
            TaxTotal = taxUsd * rate,
            GrandTotal = grandTotalUsd * rate,
            GrandTotalUsd = grandTotalUsd,
            WebsiteClientAddressID = address.WebsiteClientAddressID,
            AddressSnapshot = $"{address.ReceiverName}, {address.AddressLine}, {address.PostalCode} ({address.Phone})",
            CouponID = coupon?.CouponID,
            ShippingMethodID = shippingMethodId,
            Note = note,
            CreatedAt = DateTime.UtcNow,
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);

        foreach (var item in cart.CartItems)
        {
            var variant = item.ProductVariant;
            var unitPriceUsd = unitUsdByItem[item.CartItemID];
            var sourceWebsiteId = variant.Product.WebsiteID;
            var unitCostUsd = variant.InventoryItems.FirstOrDefault(i => i.WebsiteID == sourceWebsiteId)?.AvgCostUsd ?? 0;

            _context.OrderItems.Add(new OrderItem
            {
                OrderID = order.OrderID,
                WebsiteID = websiteId,
                SourceWebsiteID = sourceWebsiteId,
                ProductVariantID = variant.ProductVariantID,
                VendorProductID = item.VendorProductID,
                VendorID = item.VendorProduct?.VendorID,
                TitleSnapshot = string.IsNullOrWhiteSpace(variant.Title)
                    ? variant.Product.Title
                    : $"{variant.Product.Title} — {variant.Title}",
                SkuSnapshot = variant.Sku,
                Quantity = item.Quantity,
                UnitPrice = unitPriceUsd * rate,
                UnitPriceUsd = unitPriceUsd,
                UnitCostUsd = unitCostUsd,
                DiscountAmount = 0,
                TotalPrice = unitPriceUsd * rate * item.Quantity,
            });

            if (item.VendorProduct is { } vp)
            {
                // Debit vendor listing stock on host side.
                var tracked = await _context.VendorProducts.FirstOrDefaultAsync(x => x.VendorProductID == vp.VendorProductID, ct);
                if (tracked is not null)
                    tracked.StockQuantity = Math.Max(0, tracked.StockQuantity - item.Quantity);

                // Reserve stock on source website for site-linked goods.
                if (vp.Vendor?.VendorType == (byte)VendorType.Site && sourceWebsiteId != websiteId)
                    await _inventory.ReserveAsync(sourceWebsiteId, variant.ProductVariantID, item.Quantity, ct);
            }
            else
            {
                await _inventory.ReserveAsync(websiteId, variant.ProductVariantID, item.Quantity, ct);
            }
        }

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderID = order.OrderID,
            FromStatus = null,
            ToStatus = (byte)OrderStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow,
        });

        if (coupon is not null)
            await _coupons.RedeemAsync(coupon.CouponID, order.OrderID, clientId, discountUsd, ct);

        await _context.SaveChangesAsync(ct);

        // Dual settlement + inter-site credit debit + mirror orders (buyer only sees host order).
        await _vendorCredit.SettleHostOrderAsync(order.OrderID, ct);

        await tx.CommitAsync(ct);

        await _cart.ClearAsync(cartId, ct);

        await _notifications.NotifySiteAdminsAsync(
            websiteId,
            AdminNotificationType.NewOrder,
            "New order",
            $"Order #{order.OrderNumber} placed — {order.GrandTotal:0.##} {order.CurrencyCode}.",
            $"/orders/{order.OrderID}",
            order.OrderID,
            ct);

        return new CheckoutResult(true, null, order.OrderID, order.OrderNumber);
    }

    private async Task<decimal> ResolveUnitPriceUsdAsync(CartItem item, int websiteId, CancellationToken ct)
    {
        if (item.VendorProduct is { } vp)
        {
            return await _currency.ResolveCatalogUnitUsdAsync(
                websiteId, vp.ReferencePrice, vp.ReferencePriceUsd, vp.OverridePriceLocal, vp.OverridePrice, ct);
        }

        var variant = item.ProductVariant;
        return await _currency.ResolveCatalogUnitUsdAsync(
            websiteId, variant.ReferencePrice, variant.ReferencePriceUsd, null, null, ct);
    }

    public async Task<Order?> GetByIdAsync(int orderId, int? clientId = null, CancellationToken ct = default)
    {
        var query = _context.Orders
            .Include(o => o.OrderItems).ThenInclude(i => i.Vendor)
            .Include(o => o.OrderStatusHistories)
            .Include(o => o.Payments)
            .Include(o => o.ShippingMethod)
            .Include(o => o.WebsiteClientAddress)
            .AsQueryable();
        if (clientId is int cid)
            query = query.Where(o => o.WebsiteClientID == cid);
        return await query.FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
    }

    public async Task<PagedResult<Order>> GetPagedAsync(int? websiteId, byte? status, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Orders.AsNoTracking().Include(o => o.WebsiteClient).AsQueryable();
        if (websiteId is int wid) q = q.Where(o => o.WebsiteID == wid);
        if (status is byte s) q = q.Where(o => o.Status == s);

        if (query.GetSearch(nameof(Order.OrderNumber)) is string orderNumber)
            q = q.Where(o => o.OrderNumber.Contains(orderNumber));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Order.CreatedAt), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);
        return new PagedResult<Order> { Items = items, TotalCount = total };
    }

    public async Task<PagedResult<Order>> GetClientHistoryAsync(int clientId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Orders.AsNoTracking().Where(o => o.WebsiteClientID == clientId);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(o => o.CreatedAt).Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new PagedResult<Order> { Items = items, TotalCount = total };
    }

    public async Task<bool> TransitionStatusAsync(int orderId, OrderStatus newStatus, int? memberId, string? note, CancellationToken ct = default)
    {
        var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null) return false;

        var fromStatus = (OrderStatus)order.Status;
        if (fromStatus == newStatus) return true;

        order.Status = (byte)newStatus;
        if (newStatus == OrderStatus.Paid && order.PaidAt is null)
            order.PaidAt = DateTime.UtcNow;

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderID = orderId,
            FromStatus = (byte)fromStatus,
            ToStatus = (byte)newStatus,
            Note = note,
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        });

        if (newStatus is OrderStatus.Paid or OrderStatus.Processing && fromStatus is OrderStatus.PendingPayment)
        {
            foreach (var item in order.OrderItems)
            {
                var stockSite = item.SourceWebsiteID > 0 ? item.SourceWebsiteID : order.WebsiteID;
                await _inventory.DecrementOnFulfillAsync(stockSite, item.ProductVariantID, item.Quantity, order.OrderID, item.OrderItemID, memberId, ct);
            }
        }
        else if (newStatus is OrderStatus.Cancelled or OrderStatus.Refunded)
        {
            if (fromStatus is OrderStatus.PendingPayment)
            {
                foreach (var item in order.OrderItems)
                {
                    var stockSite = item.SourceWebsiteID > 0 ? item.SourceWebsiteID : order.WebsiteID;
                    await _inventory.ReleaseReservationAsync(stockSite, item.ProductVariantID, item.Quantity, ct);
                }
            }

            await _vendorCredit.ReverseHostOrderAsync(orderId, ct);
        }

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ClientHasPaidOrderForProductAsync(int clientId, int productId, CancellationToken ct = default) =>
        await _context.Orders
            .Where(o => o.WebsiteClientID == clientId && o.Status >= (byte)OrderStatus.Paid)
            .SelectMany(o => o.OrderItems)
            .AnyAsync(i => i.ProductVariant.ProductID == productId, ct);

    private static string GenerateOrderNumber(int websiteId) =>
        $"{websiteId}-{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(100, 999)}";
}
