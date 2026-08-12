using Dotnetable.Application.DTOs;
using Dotnetable.Application.Email;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventory;
    private readonly IVendorProductService _vendorProducts;
    private readonly IShippingService _shipping;
    private readonly ITaxService _tax;
    private readonly ICouponService _coupons;
    private readonly ICurrencyConversionService _currency;
    private readonly ICartService _cart;
    private readonly IAdminNotificationService _notifications;
    private readonly IVendorCreditService _vendorCredit;
    private readonly IDigitalDeliveryService _digitalDelivery;
    private readonly IEmailService _email;
    private readonly ISmsSender _sms;
    private readonly IWhatsAppSender _whatsApp;
    private readonly IFinancialLedgerService _ledger;
    private readonly IStockDocumentService _stockDocs;
    private readonly IWarehouseService _warehouses;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        AppDbContext context, IInventoryService inventory, IVendorProductService vendorProducts,
        IShippingService shipping, ITaxService tax, ICouponService coupons, ICurrencyConversionService currency,
        ICartService cart, IAdminNotificationService notifications, IVendorCreditService vendorCredit,
        IDigitalDeliveryService digitalDelivery,
        IEmailService email, ISmsSender sms, IWhatsAppSender whatsApp,
        IFinancialLedgerService ledger, IStockDocumentService stockDocs, IWarehouseService warehouses,
        ILogger<OrderService> logger)
    {
        _context = context;
        _inventory = inventory;
        _vendorProducts = vendorProducts;
        _shipping = shipping;
        _tax = tax;
        _coupons = coupons;
        _currency = currency;
        _cart = cart;
        _notifications = notifications;
        _vendorCredit = vendorCredit;
        _digitalDelivery = digitalDelivery;
        _email = email;
        _sms = sms;
        _whatsApp = whatsApp;
        _ledger = ledger;
        _stockDocs = stockDocs;
        _warehouses = warehouses;
        _logger = logger;
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

        // Stock / vendor listing re-validation (available = on-hand − reserved).
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
                }

                if (IVendorProductService.Available(vp) < item.Quantity)
                    return new CheckoutResult(false, $"Insufficient vendor stock for {item.ProductVariant.Product.Title}.", null, null);
            }
            else
            {
                // No warehouse-only sales: every line must be sold by a store listing.
                return new CheckoutResult(false, $"No store stock assigned for {item.ProductVariant.Product.Title}.", null, null);
            }
        }

        var requiresShipping = cart.CartItems.Any(i => i.ProductVariant.Product.RequiresShipping);
        var totalWeight = cart.CartItems.Sum(i =>
            i.ProductVariant.Product.RequiresShipping
                ? (i.ProductVariant.Weight ?? 0) * i.Quantity
                : 0m);

        var subtotalUsd = cart.CartItems.Sum(i => unitUsdByItem[i.CartItemID] * i.Quantity);
        decimal cartSubtotalLocal = subtotalUsd;
        try
        {
            cartSubtotalLocal = (await _currency.ToDisplayAsync(websiteId, subtotalUsd, null, ct)).Amount;
        }
        catch (InvalidOperationException)
        {
            // Keep USD as stand-in when rates are missing.
        }

        decimal shippingUsd = 0;
        int? resolvedShippingMethodId = null;
        if (requiresShipping)
        {
            if (shippingMethodId <= 0)
                return new CheckoutResult(false, "A shipping method is required for physical items.", null, null);

            var shippingOptions = await _shipping.GetAvailableWithPricesAsync(
                websiteId, address.CountryId, stateId, address.CityId, totalWeight, cartSubtotalLocal, ct);
            var shippingOption = shippingOptions.FirstOrDefault(o => o.Method.ShippingMethodID == shippingMethodId);
            if (shippingOption is null)
                return new CheckoutResult(false, "Selected shipping method is not available for this address.", null, null);

            shippingUsd = shippingOption.PriceUsd;
            resolvedShippingMethodId = shippingMethodId;
        }

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

        // Tax base after merchandise discount; shipping taxed per site TaxOnShipping + rate flags.
        var taxableMerchandise = Math.Max(0, subtotalUsd - discountUsd);
        var taxResult = await _tax.ComputeTaxDetailedAsync(
            websiteId, address.CountryId, stateId, taxableMerchandise, shippingUsd, ct);
        var taxUsd = taxResult.TaxAmount;

        // Exclusive tax is added on top; inclusive tax is already in prices (extracted for reporting only).
        var grandTotalUsd = taxResult.PricesIncludeTax
            ? subtotalUsd + shippingUsd - discountUsd
            : subtotalUsd + shippingUsd + taxUsd - discountUsd;
        var (resolvedCurrency, rate) = await _currency.GetActiveRateAsync(websiteId, currencyCode, ct);

        // EnableRetryOnFailure requires user transactions to run inside the execution strategy.
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            // AppDbContext is Transient — nested services get their own connection unless ambient is set.
            // Without this, SettleHostOrderAsync blocks on the uncommitted order row until SQL timeout.
            using var ambient = AmbientDbContext.Use(_context);

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
                PricesIncludeTax = taxResult.PricesIncludeTax,
                TaxBreakdownJson = taxResult.BreakdownJson,
                GrandTotal = grandTotalUsd * rate,
                GrandTotalUsd = grandTotalUsd,
                WebsiteClientAddressID = address.WebsiteClientAddressID,
                AddressSnapshot = $"{address.ReceiverName}, {address.AddressLine}, {address.PostalCode} ({address.Phone})",
                CouponID = coupon?.CouponID,
                ShippingMethodID = resolvedShippingMethodId,
                Note = note,
                SalesChannel = (byte)OrderSalesChannel.Online,
                ReportToTax = true,
                MarkupTotal = 0,
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

                var unitLocal = unitPriceUsd * rate;
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
                    UnitPrice = unitLocal,
                    UnitPriceUsd = unitPriceUsd,
                    UnitCostUsd = unitCostUsd,
                    CatalogUnitPrice = unitLocal,
                    UnitMarkup = 0,
                    DiscountAmount = 0,
                    TotalPrice = unitLocal * item.Quantity,
                });

                // Reserve only at checkout; on-hand is deducted after payment.
                // Unlimited digital listings skip inventory reservation entirely.
                if (item.VendorProduct is { } vp)
                {
                    if (!await _vendorProducts.ReserveAsync(vp.VendorProductID, item.Quantity, ct))
                    {
                        await tx.RollbackAsync(ct);
                        return new CheckoutResult(false, $"Insufficient vendor stock for {item.ProductVariant.Product.Title}.", null, null);
                    }

                    if (IVendorProductService.IsUnlimited(vp))
                        continue;

                    // Site-linked: reserve physical stock on the source website inventory.
                    // Host display/member listings: reserve host inventory (materialised sum of store stocks).
                    var isCrossSite = vp.Vendor?.VendorType == (byte)VendorType.Site && sourceWebsiteId != websiteId;
                    var reserveSiteId = isCrossSite ? sourceWebsiteId : websiteId;
                    if (!isCrossSite)
                        await _vendorProducts.SyncInventoryOnHandFromListingsAsync(websiteId, variant.ProductVariantID, ct);

                    if (!await _inventory.ReserveAsync(reserveSiteId, variant.ProductVariantID, item.Quantity, ct))
                    {
                        await _vendorProducts.ReleaseReservationAsync(vp.VendorProductID, item.Quantity, ct);
                        await tx.RollbackAsync(ct);
                        return new CheckoutResult(false, $"Insufficient stock for {item.ProductVariant.Product.Title}.", null, null);
                    }

                    // WMS sites: also reserve warehouse bins at checkout (not only listings / inventory book).
                    if (!isCrossSite)
                    {
                        var whId = await _warehouses.GetDefaultWarehouseIdAsync(websiteId, ct);
                        if (whId is int warehouseId)
                        {
                            if (!await _warehouses.ReserveAsync(warehouseId, variant.ProductVariantID, item.Quantity, ct))
                            {
                                await _inventory.ReleaseReservationAsync(reserveSiteId, variant.ProductVariantID, item.Quantity, ct);
                                await _vendorProducts.ReleaseReservationAsync(vp.VendorProductID, item.Quantity, ct);
                                await tx.RollbackAsync(ct);
                                return new CheckoutResult(false,
                                    $"Insufficient warehouse stock for {item.ProductVariant.Product.Title}.", null, null);
                            }
                        }
                    }
                }
                else
                {
                    await tx.RollbackAsync(ct);
                    return new CheckoutResult(false, $"No store stock assigned for {item.ProductVariant.Product.Title}.", null, null);
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
        });
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
            .Include(o => o.OrderItems).ThenInclude(i => i.ProductVariant)
            .Include(o => o.OrderStatusHistories)
            .Include(o => o.Payments).ThenInclude(p => p.CreatedByMember)
            .Include(o => o.Payments).ThenInclude(p => p.VerifiedByMember)
            .Include(o => o.Payments).ThenInclude(p => p.WebsiteClient)
            .Include(o => o.ShippingMethod)
            .Include(o => o.WebsiteClientAddress)
            .Include(o => o.WebsiteClient)
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

    public async Task<IReadOnlyDictionary<byte, int>> GetStatusCountsAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.Orders.AsNoTracking().AsQueryable();
        if (websiteId is int wid) q = q.Where(o => o.WebsiteID == wid);

        var rows = await q
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Status, r => r.Count);
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
        // Share one context with inventory / digital / credit services (Transient AppDbContext).
        using var ambient = AmbientDbContext.Use(_context);

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
            // When WMS warehouses exist: create Submitted outbound pick doc and skip immediate inventory leave
            // (stock leaves when outbound is Posted — typically on ship).
            var useWms = await _stockDocs.WebsiteHasWarehouseAsync(order.WebsiteID, ct);

            foreach (var item in order.OrderItems)
            {
                // Free-form / no-listing admin lines skip inventory.
                if (item.ProductVariantID is null)
                    continue;

                // Deduct store listing after payment (was only reserved at checkout).
                if (item.VendorProductID is int vendorProductId)
                    await _vendorProducts.CommitSaleAsync(vendorProductId, item.Quantity, ct);

                // Only touch physical inventory when a store listing reserved stock.
                if (item.VendorProductID is null)
                    continue;

                if (useWms)
                    continue;

                var stockSite = ResolveStockSite(item, order);
                await _inventory.DecrementOnFulfillAsync(stockSite, item.ProductVariantID.Value, item.Quantity, order.OrderID, item.OrderItemID, memberId, ct);
                await _vendorProducts.SyncInventoryOnHandFromListingsAsync(order.WebsiteID, item.ProductVariantID.Value, ct);
            }

            if (useWms)
            {
                try
                {
                    var (ok, err, _) = await _stockDocs.EnsureOutboundForOrderAsync(orderId, memberId, ct);
                    if (!ok)
                        _logger.LogWarning("EnsureOutboundForOrder {OrderId} failed: {Error}", orderId, err);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "EnsureOutboundForOrder {OrderId} threw", orderId);
                }
            }

            // Permanent digital library entitlements (idempotent).
            await _digitalDelivery.GrantForOrderAsync(orderId, ct);
        }
        else if (newStatus == OrderStatus.Shipped
                 && (fromStatus is OrderStatus.Paid or OrderStatus.Processing))
        {
            void RevertShipTransition()
            {
                order.Status = (byte)fromStatus;
                foreach (var h in _context.ChangeTracker.Entries<OrderStatusHistory>()
                             .Where(e => e.State == EntityState.Added && e.Entity.OrderID == orderId
                                         && e.Entity.ToStatus == (byte)OrderStatus.Shipped)
                             .ToList())
                    h.State = EntityState.Detached;
            }

            var (canShip, shipErr) = await _stockDocs.CanShipOrderAsync(orderId, ct);
            if (!canShip)
            {
                RevertShipTransition();
                _logger.LogWarning("Ship blocked for order {OrderId}: {Error}", orderId, shipErr);
                return false;
            }

            try
            {
                var (ok, err) = await _stockDocs.PostOutboundForOrderAsync(orderId, memberId, ct);
                if (!ok)
                {
                    RevertShipTransition();
                    _logger.LogWarning("PostOutboundForOrder {OrderId} failed: {Error}", orderId, err);
                    return false;
                }
            }
            catch (Exception ex)
            {
                RevertShipTransition();
                _logger.LogError(ex, "PostOutboundForOrder {OrderId} threw", orderId);
                return false;
            }
        }
        else if (newStatus is OrderStatus.Cancelled or OrderStatus.Refunded)
        {
            if (fromStatus is OrderStatus.PendingPayment)
            {
                var whId = await _warehouses.GetDefaultWarehouseIdAsync(order.WebsiteID, ct);
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductVariantID is null)
                        continue;

                    if (item.VendorProductID is int vendorProductId)
                        await _vendorProducts.ReleaseReservationAsync(vendorProductId, item.Quantity, ct);

                    if (item.VendorProductID is null)
                        continue;

                    var stockSite = ResolveStockSite(item, order);
                    await _inventory.ReleaseReservationAsync(stockSite, item.ProductVariantID.Value, item.Quantity, ct);

                    if (whId is int warehouseId)
                        await _warehouses.ReleaseReservationAsync(warehouseId, item.ProductVariantID.Value, item.Quantity, ct);
                }
            }
            else if (fromStatus is OrderStatus.Paid or OrderStatus.Processing)
            {
                // Unposted pick: release warehouse reservation held since checkout.
                var outbound = await _stockDocs.GetOutboundForOrderAsync(orderId, ct);
                if (outbound is null || outbound.Status is not (byte)StockDocumentStatus.Posted)
                {
                    var whId = await _warehouses.GetDefaultWarehouseIdAsync(order.WebsiteID, ct);
                    if (whId is int warehouseId)
                    {
                        foreach (var item in order.OrderItems)
                        {
                            if (item.ProductVariantID is null || item.VendorProductID is null) continue;
                            await _warehouses.ReleaseReservationAsync(warehouseId, item.ProductVariantID.Value, item.Quantity, ct);
                            var stockSite = ResolveStockSite(item, order);
                            await _inventory.ReleaseReservationAsync(stockSite, item.ProductVariantID.Value, item.Quantity, ct);
                        }
                    }
                }
            }

            // Cancel unposted WMS outbound pick for this order (posted stock reversed via Return doc on refund).
            try
            {
                var outbound = await _stockDocs.GetOutboundForOrderAsync(orderId, ct);
                if (outbound is not null
                    && outbound.Status is not (
                        (byte)StockDocumentStatus.Posted
                        or (byte)StockDocumentStatus.Cancelled
                        or (byte)StockDocumentStatus.Rejected))
                {
                    await _stockDocs.CancelAsync(outbound.StockDocumentID, memberId,
                        $"Order {(OrderStatus)newStatus}", ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not cancel outbound for order {OrderId}", orderId);
            }

            // Revoke permanent digital access on cancel/refund.
            var digitalAssets = await _context.OrderDigitalAssets
                .Where(a => a.OrderID == orderId && a.IsActive)
                .ToListAsync(ct);
            foreach (var asset in digitalAssets)
                asset.IsActive = false;

            await _vendorCredit.ReverseHostOrderAsync(orderId, ct);
        }

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(bool Success, string? Error)> UpdateFulfillmentAsync(
        int orderId,
        OrderPreparationStatus? preparationStatus,
        OrderShippingStatus? shippingStatus,
        string? shippingTrackingCode,
        int? memberId,
        string? note = null,
        bool syncOrderStatus = true,
        CancellationToken ct = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null)
            return (false, "Order not found.");

        var previousTracking = order.ShippingTrackingCode;
        var changes = new List<string>();
        var fromOrderStatus = (OrderStatus)order.Status;

        if (preparationStatus is OrderPreparationStatus prep
            && order.PreparationStatus != (byte)prep)
        {
            changes.Add($"Preparation: {(OrderPreparationStatus)order.PreparationStatus} → {prep}");
            order.PreparationStatus = (byte)prep;
        }

        if (shippingStatus is OrderShippingStatus ship
            && order.ShippingStatus != (byte)ship)
        {
            changes.Add($"Shipping: {(OrderShippingStatus)order.ShippingStatus} → {ship}");
            order.ShippingStatus = (byte)ship;
            if (ship is OrderShippingStatus.Shipped or OrderShippingStatus.InTransit or OrderShippingStatus.Delivered
                && order.ShippedAt is null)
            {
                order.ShippedAt = DateTime.UtcNow;
            }
        }

        if (shippingTrackingCode is not null)
        {
            var code = shippingTrackingCode.Trim();
            if (code.Length > 100)
                return (false, "Tracking code is too long (max 100 characters).");

            var normalized = string.IsNullOrWhiteSpace(code) ? null : code;
            if (!string.Equals(order.ShippingTrackingCode, normalized, StringComparison.Ordinal))
            {
                changes.Add(normalized is null
                    ? "Tracking code cleared"
                    : $"Tracking code set to {normalized}");
                order.ShippingTrackingCode = normalized;
            }
        }

        // Align main order lifecycle when fulfillment moves forward (optional).
        if (syncOrderStatus
            && fromOrderStatus is not (OrderStatus.Cancelled or OrderStatus.Refunded))
        {
            OrderStatus? target = null;
            if (order.ShippingStatus is (byte)OrderShippingStatus.Delivered
                && fromOrderStatus is OrderStatus.Paid or OrderStatus.Processing or OrderStatus.Shipped)
            {
                target = OrderStatus.Completed;
            }
            else if (order.ShippingStatus is (byte)OrderShippingStatus.Shipped
                         or (byte)OrderShippingStatus.InTransit
                     && fromOrderStatus is OrderStatus.Paid or OrderStatus.Processing)
            {
                target = OrderStatus.Shipped;
            }
            else if (order.PreparationStatus is (byte)OrderPreparationStatus.Preparing
                         or (byte)OrderPreparationStatus.ReadyToShip
                         or (byte)OrderPreparationStatus.Pending
                     && fromOrderStatus is OrderStatus.Paid)
            {
                target = OrderStatus.Processing;
            }

            if (target is OrderStatus next && next != fromOrderStatus)
            {
                var syncNoteParts = changes.Count > 0
                    ? string.Join("; ", changes)
                    : "Fulfillment update";
                if (!string.IsNullOrWhiteSpace(note))
                    syncNoteParts = $"{syncNoteParts}. {note}";
                if (syncNoteParts.Length > 480)
                    syncNoteParts = syncNoteParts[..480];

                // Reuse stock / digital side-effects of the lifecycle transition.
                var ok = await TransitionStatusAsync(orderId, next, memberId, syncNoteParts, ct);
                if (!ok)
                    return (false, "Could not sync order status.");

                // TransitionStatusAsync saves Status; re-apply fulfillment fields on the tracked row.
                order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
                if (order is null)
                    return (false, "Order not found.");

                if (preparationStatus is OrderPreparationStatus prep2)
                    order.PreparationStatus = (byte)prep2;
                if (shippingStatus is OrderShippingStatus ship2)
                {
                    order.ShippingStatus = (byte)ship2;
                    if (ship2 is OrderShippingStatus.Shipped or OrderShippingStatus.InTransit or OrderShippingStatus.Delivered
                        && order.ShippedAt is null)
                        order.ShippedAt = DateTime.UtcNow;
                }
                if (shippingTrackingCode is not null)
                {
                    var code = shippingTrackingCode.Trim();
                    order.ShippingTrackingCode = string.IsNullOrWhiteSpace(code) ? null : code;
                }

                await _context.SaveChangesAsync(ct);
                var (postOk, postErr) = await TryPostOutboundOnShipAsync(orderId, order.ShippingStatus, memberId, ct);
                if (!postOk)
                    return (false, postErr);
                await NotifyTrackingCodeIfChangedAsync(orderId, previousTracking, order.ShippingTrackingCode, ct);
                return (true, null);
            }
        }

        if (changes.Count == 0 && string.IsNullOrWhiteSpace(note))
            return (true, null);

        // Preflight warehouse when shipping advances (block UI before history write).
        if (shippingStatus is OrderShippingStatus.Shipped or OrderShippingStatus.InTransit or OrderShippingStatus.Delivered)
        {
            var (canShip, shipErr) = await _stockDocs.CanShipOrderAsync(orderId, ct);
            if (!canShip)
                return (false, shipErr ?? "Insufficient warehouse stock to ship. Prepare stock or refund the customer.");
        }

        var historyNote = string.IsNullOrWhiteSpace(note)
            ? string.Join("; ", changes)
            : string.IsNullOrEmpty(string.Join("; ", changes))
                ? note!
                : $"{string.Join("; ", changes)}. {note}";

        // Fulfillment-only change: same order Status, note still useful on the timeline.
        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderID = orderId,
            FromStatus = order.Status,
            ToStatus = order.Status,
            Note = historyNote.Length > 500 ? historyNote[..500] : historyNote,
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync(ct);
        var (postOk2, postErr2) = await TryPostOutboundOnShipAsync(orderId, order.ShippingStatus, memberId, ct);
        if (!postOk2)
            return (false, postErr2);
        await NotifyTrackingCodeIfChangedAsync(orderId, previousTracking, order.ShippingTrackingCode, ct);
        return (true, null);
    }

    /// <summary>
    /// When shipping advances, post the WMS outbound (idempotent if already posted).
    /// Returns error when warehouse stock is insufficient so callers can block the UI.
    /// </summary>
    private async Task<(bool Success, string? Error)> TryPostOutboundOnShipAsync(
        int orderId, byte shippingStatus, int? memberId, CancellationToken ct)
    {
        if (shippingStatus is not (
            (byte)OrderShippingStatus.Shipped
            or (byte)OrderShippingStatus.InTransit
            or (byte)OrderShippingStatus.Delivered))
            return (true, null);

        var (canShip, shipErr) = await _stockDocs.CanShipOrderAsync(orderId, ct);
        if (!canShip)
        {
            _logger.LogWarning("Ship blocked for order {OrderId}: {Error}", orderId, shipErr);
            return (false, shipErr ?? "Insufficient warehouse stock to ship.");
        }

        try
        {
            var (ok, err) = await _stockDocs.PostOutboundForOrderAsync(orderId, memberId, ct);
            if (!ok)
            {
                _logger.LogWarning("PostOutboundForOrder {OrderId} on fulfillment failed: {Error}", orderId, err);
                return (false, err ?? "Could not post warehouse outbound.");
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostOutboundForOrder {OrderId} on fulfillment threw", orderId);
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// When a non-empty tracking code is newly set or changed, notify the customer on every
    /// configured channel (email / SMS / WhatsApp). Failures are logged and never fail fulfillment.
    /// Message language follows the website default language.
    /// </summary>
    private async Task NotifyTrackingCodeIfChangedAsync(
        int orderId, string? previousTracking, string? newTracking, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(newTracking))
            return;
        if (string.Equals(previousTracking?.Trim(), newTracking.Trim(), StringComparison.Ordinal))
            return;

        try
        {
            await NotifyShipmentTrackingAsync(orderId, newTracking.Trim(), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send shipment tracking notifications for order {OrderId}", orderId);
        }
    }

    private async Task NotifyShipmentTrackingAsync(int orderId, string trackingCode, CancellationToken ct)
    {
        var order = await _context.Orders.AsNoTracking()
            .Include(o => o.WebsiteClient)
            .Include(o => o.Website)
            .Include(o => o.ShippingMethod)
            .FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order?.WebsiteClient is null || order.Website is null)
            return;

        var client = order.WebsiteClient;
        var website = order.Website;
        var lang = website.DefaultLanguageCode;
        var siteName = website.BrandName ?? website.TradeName ?? string.Empty;
        var customerName = string.Join(' ', new[] { client.Givenname, client.Surname }.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (string.IsNullOrWhiteSpace(customerName))
            customerName = client.Email ?? client.Cellphone ?? string.Empty;

        var shippingMethod = order.ShippingMethod is null
            ? "—"
            : (!string.IsNullOrWhiteSpace(order.ShippingMethod.Title)
                ? order.ShippingMethod.Title
                : (order.ShippingMethod.CarrierName ?? "—"));

        // ── Email (Sales account / OrderShipped template) ─────────────
        if (!string.IsNullOrWhiteSpace(client.Email) && await _email.IsConfiguredAsync(order.WebsiteID, ct))
        {
            try
            {
                await _email.SendTemplateAsync(
                    order.WebsiteID,
                    EmailTemplateKeys.OrderShipped,
                    client.Email!,
                    new Dictionary<string, string>
                    {
                        ["Name"] = customerName,
                        ["OrderNumber"] = order.OrderNumber,
                        ["TrackingCode"] = trackingCode,
                        ["ShippingMethod"] = shippingMethod,
                    },
                    languageCode: null, // → website DefaultLanguageCode (+ FA built-in when applicable)
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Shipment tracking email failed for order {OrderId}", orderId);
            }
        }

        var plain = OrderShipmentMessages.PlainText(
            lang, siteName, customerName, order.OrderNumber, trackingCode,
            string.IsNullOrWhiteSpace(shippingMethod) || shippingMethod == "—" ? null : shippingMethod);

        var country = client.CountryCode ?? string.Empty;
        var phone = client.Cellphone;

        // ── SMS ───────────────────────────────────────────────────────
        if (_sms.IsConfigured && !string.IsNullOrWhiteSpace(phone))
        {
            try
            {
                await _sms.SendAsync(country, phone!, plain, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Shipment tracking SMS failed for order {OrderId}", orderId);
            }
        }

        // ── WhatsApp ──────────────────────────────────────────────────
        if (_whatsApp.IsConfigured && !string.IsNullOrWhiteSpace(phone))
        {
            try
            {
                await _whatsApp.SendAsync(country, phone!, plain, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Shipment tracking WhatsApp failed for order {OrderId}", orderId);
            }
        }
    }

    public async Task<bool> ClientHasPaidOrderForProductAsync(int clientId, int productId, CancellationToken ct = default) =>
        await _context.Orders
            .Where(o => o.WebsiteClientID == clientId && o.Status >= (byte)OrderStatus.Paid)
            .SelectMany(o => o.OrderItems)
            .AnyAsync(i => i.ProductVariantID != null && i.ProductVariant!.ProductID == productId, ct);

    private sealed record AdminPreparedLine(
        AdminOrderLineRequest Req,
        ProductVariant? Variant,
        VendorProduct? Vp,
        decimal CatalogLocal,
        decimal ChargedLocal,
        string Title,
        string Sku,
        int SourceWebsiteId,
        decimal UnitCostUsd,
        bool TrackStock);

    /// <summary>
    /// Site-linked vendor lines reserve/fulfill physical stock on the source site;
    /// other marketplace and direct lines use the host website inventory.
    /// </summary>
    private static int ResolveStockSite(OrderItem item, Order order)
    {
        // Prefer source when it differs from host (cross-site site-vendor sale).
        if (item.SourceWebsiteID > 0 && item.SourceWebsiteID != order.WebsiteID)
            return item.SourceWebsiteID;
        return order.WebsiteID;
    }

    public async Task<AdminCreateOrderResult> AdminCreateAsync(
        AdminCreateOrderRequest request, int memberId, CancellationToken ct = default)
    {
        if (request.WebsiteId <= 0)
            return new AdminCreateOrderResult(false, "Website is required.", null, null);
        if (request.WebsiteClientId <= 0)
            return new AdminCreateOrderResult(false, "Customer is required.", null, null);
        if (request.Lines is null || request.Lines.Count == 0)
            return new AdminCreateOrderResult(false, "Add at least one product line.", null, null);
        if (request.Lines.Any(l => l.Quantity <= 0 || l.ChargedUnitPrice < 0))
            return new AdminCreateOrderResult(false, "Each line needs quantity > 0 and a non-negative price.", null, null);
        if (request.Lines.Any(l =>
                l.ProductVariantId is null or <= 0
                && string.IsNullOrWhiteSpace(l.CustomTitle)))
            return new AdminCreateOrderResult(false, "Each line needs a catalog product or a free-form title.", null, null);

        var website = await _context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteID == request.WebsiteId, ct);
        if (website is null)
            return new AdminCreateOrderResult(false, "Website not found.", null, null);

        var client = await _context.WebsiteClients
            .FirstOrDefaultAsync(c => c.WebsiteClientID == request.WebsiteClientId && c.WebsiteID == request.WebsiteId, ct);
        if (client is null)
            return new AdminCreateOrderResult(false, "Customer not found on this website.", null, null);
        if (!client.Active)
            return new AdminCreateOrderResult(false, "Customer is inactive.", null, null);

        // Resolve address (existing or new).
        WebsiteClientAddress? address = null;
        if (request.WebsiteClientAddressId is int existingAddressId)
        {
            address = await _context.WebsiteClientAddresses
                .Include(a => a.City)
                .FirstOrDefaultAsync(a =>
                    a.WebsiteClientAddressID == existingAddressId && a.WebsiteClientID == client.WebsiteClientID, ct);
            if (address is null)
                return new AdminCreateOrderResult(false, "Address not found for this customer.", null, null);
        }
        else if (request.NewAddress is { } na && !string.IsNullOrWhiteSpace(na.AddressLine))
        {
            address = new WebsiteClientAddress
            {
                WebsiteClientID = client.WebsiteClientID,
                Title = string.IsNullOrWhiteSpace(na.Title) ? null : na.Title.Trim(),
                ReceiverName = string.IsNullOrWhiteSpace(na.ReceiverName)
                    ? $"{client.Givenname} {client.Surname}".Trim()
                    : na.ReceiverName.Trim(),
                CountryId = na.CountryId,
                CityId = na.CityId,
                AddressLine = na.AddressLine.Trim(),
                PostalCode = string.IsNullOrWhiteSpace(na.PostalCode) ? null : na.PostalCode.Trim(),
                Phone = string.IsNullOrWhiteSpace(na.Phone) ? client.Cellphone : na.Phone.Trim(),
                IsDefault = false,
            };
            if (na.SaveToClient)
            {
                _context.WebsiteClientAddresses.Add(address);
                await _context.SaveChangesAsync(ct);
            }
        }

        var addressSnapshot = address is null
            ? null
            : $"{address.ReceiverName}, {address.AddressLine}, {address.PostalCode} ({address.Phone})";

        // Prepare lines: catalog (+ optional listing) or free-form title-only.
        var prepared = new List<AdminPreparedLine>();
        foreach (var line in request.Lines)
        {
            if (line.ProductVariantId is int variantId and > 0)
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .Include(v => v.InventoryItems)
                    .FirstOrDefaultAsync(v => v.ProductVariantID == variantId && v.IsActive, ct);
                if (variant is null)
                    return new AdminCreateOrderResult(false, $"Product variant #{variantId} not found.", null, null);

                VendorProduct? vp = null;
                if (line.VendorProductId is int vpId)
                {
                    vp = await _context.VendorProducts
                        .Include(x => x.Vendor)
                        .FirstOrDefaultAsync(x =>
                            x.VendorProductID == vpId
                            && x.ProductVariantID == variantId
                            && x.WebsiteID == request.WebsiteId, ct);
                }
                else
                {
                    vp = await _context.VendorProducts
                        .Include(x => x.Vendor)
                        .Where(x => x.WebsiteID == request.WebsiteId
                                    && x.ProductVariantID == variantId
                                    && x.IsActive
                                    && (x.Vendor == null || x.Vendor.IsActive))
                        .OrderByDescending(x => x.StockQuantity)
                        .FirstOrDefaultAsync(ct);
                }

                // Listing is optional for admin orders (social sales without warehouse listing).
                if (vp is not null)
                {
                    if (!vp.IsActive || vp.Vendor is null || !vp.Vendor.IsActive)
                        return new AdminCreateOrderResult(false, $"Store listing inactive for {variant.Product.Title} ({variant.Sku}).", null, null);
                    if (IVendorProductService.Available(vp) < line.Quantity)
                        return new AdminCreateOrderResult(false, $"Insufficient stock for {variant.Product.Title} ({variant.Sku}).", null, null);
                }

                decimal catalogLocal;
                if (vp is not null)
                {
                    (catalogLocal, _) = await _currency.ResolveCatalogUnitLocalAsync(
                        request.WebsiteId, vp.ReferencePrice, vp.ReferencePriceUsd,
                        vp.OverridePriceLocal, vp.OverridePrice, ct);
                }
                else
                {
                    (catalogLocal, _) = await _currency.ResolveCatalogUnitLocalAsync(
                        request.WebsiteId, variant.ReferencePrice, variant.ReferencePriceUsd, null, null, ct);
                }
                if (line.CatalogUnitPrice is >= 0)
                    catalogLocal = line.CatalogUnitPrice.Value;

                var title = string.IsNullOrWhiteSpace(variant.Title)
                    ? variant.Product.Title
                    : $"{variant.Product.Title} — {variant.Title}";
                var sourceWebsiteId = variant.Product.WebsiteID;
                var unitCostUsd = variant.InventoryItems.FirstOrDefault(i => i.WebsiteID == sourceWebsiteId)?.AvgCostUsd ?? 0;

                prepared.Add(new AdminPreparedLine(
                    line, variant, vp, catalogLocal, line.ChargedUnitPrice,
                    title, variant.Sku, sourceWebsiteId, unitCostUsd,
                    TrackStock: vp is not null));
            }
            else
            {
                var title = line.CustomTitle!.Trim();
                if (title.Length > 300) title = title[..300];
                var sku = string.IsNullOrWhiteSpace(line.CustomSku) ? "CUSTOM" : line.CustomSku.Trim();
                if (sku.Length > 100) sku = sku[..100];
                var catalogLocal = line.CatalogUnitPrice is >= 0
                    ? line.CatalogUnitPrice.Value
                    : line.ChargedUnitPrice;

                prepared.Add(new AdminPreparedLine(
                    line, null, null, catalogLocal, line.ChargedUnitPrice,
                    title, sku, request.WebsiteId, 0, TrackStock: false));
            }
        }

        var (resolvedCurrency, rate) = await _currency.GetActiveRateAsync(request.WebsiteId, request.CurrencyCode, ct);
        if (rate <= 0) rate = 1m;

        decimal ToUsd(decimal local) => Math.Round(local / rate, 4, MidpointRounding.AwayFromZero);

        var subtotalLocal = prepared.Sum(p => p.ChargedLocal * p.Req.Quantity);
        var markupTotalLocal = prepared.Sum(p => Math.Max(0, p.ChargedLocal - p.CatalogLocal) * p.Req.Quantity);

        // Free-form lines always need shipping address path when any shipping method is chosen;
        // catalog lines use product RequiresShipping.
        var requiresShipping = prepared.Any(p =>
            p.Variant is null || p.Variant.Product.RequiresShipping);
        var totalWeight = prepared.Sum(p =>
            p.Variant is { Product.RequiresShipping: true }
                ? (p.Variant.Weight ?? 0) * p.Req.Quantity
                : 0m);

        decimal shippingLocal = request.ShippingTotal ?? 0;
        int? shippingMethodId = request.ShippingMethodId;
        if (requiresShipping && address is not null && shippingMethodId is int smId && smId > 0 && request.ShippingTotal is null)
        {
            if (address.City is null && address.CityId is int shipCityId)
                address.City = await _context.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.CityID == shipCityId, ct);

            var stateId = address.City?.StateID;
            var options = await _shipping.GetAvailableWithPricesAsync(
                request.WebsiteId, address.CountryId, stateId, address.CityId, totalWeight, subtotalLocal, ct);
            var opt = options.FirstOrDefault(o => o.Method.ShippingMethodID == smId);
            if (opt is not null)
                shippingLocal = (await _currency.ToDisplayAsync(request.WebsiteId, opt.PriceUsd, resolvedCurrency, ct)).Amount;
            else
                shippingLocal = 0;
        }
        else if (!requiresShipping)
        {
            shippingLocal = 0;
            shippingMethodId = null;
        }

        var reportToTax = request.ReportToTax ?? (
            request.SalesChannel == OrderSalesChannel.Online
                ? true
                : website.ReportOfflineOrdersToTax);

        decimal taxLocal = 0;
        bool pricesIncludeTax = website.PricesIncludeTax;
        string? taxBreakdown = null;
        if (reportToTax && website.TaxEnabled && address is not null)
        {
            if (address.City is null && address.CityId is int cityId)
                address.City = await _context.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.CityID == cityId, ct);

            var stateId = address.City?.StateID;
            var taxResult = await _tax.ComputeTaxDetailedAsync(
                request.WebsiteId, address.CountryId, stateId,
                ToUsd(subtotalLocal), ToUsd(shippingLocal), ct);
            taxLocal = taxResult.TaxAmount * rate;
            pricesIncludeTax = taxResult.PricesIncludeTax;
            taxBreakdown = taxResult.BreakdownJson;
        }
        else if (!reportToTax)
        {
            // Explicitly keep tax out of filings for offline/unreported channels.
            taxLocal = 0;
            taxBreakdown = null;
        }

        var grandLocal = pricesIncludeTax && reportToTax
            ? subtotalLocal + shippingLocal
            : subtotalLocal + shippingLocal + taxLocal;
        var grandUsd = ToUsd(grandLocal);

        // Persist ephemeral address if not saved to client yet (before transaction).
        if (address is not null && address.WebsiteClientAddressID == 0 && request.NewAddress is { SaveToClient: false })
        {
            // Snapshot only — do not insert address row.
            address = null;
        }

        // EnableRetryOnFailure requires user transactions to run inside the execution strategy.
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
        await using var tx = await _context.Database.BeginTransactionAsync(ct);
        // AppDbContext is Transient — nested services get their own connection unless ambient is set.
        // Without this, SettleHostOrderAsync blocks on the uncommitted order row until SQL timeout.
        using var ambient = AmbientDbContext.Use(_context);

        var order = new Order
        {
            WebsiteID = request.WebsiteId,
            OrderNumber = GenerateOrderNumber(request.WebsiteId),
            WebsiteClientID = client.WebsiteClientID,
            Status = (byte)OrderStatus.PendingPayment,
            CurrencyCode = resolvedCurrency,
            ExchangeRateToUsd = rate,
            SubTotal = subtotalLocal,
            DiscountTotal = 0,
            ShippingTotal = shippingLocal,
            TaxTotal = taxLocal,
            PricesIncludeTax = pricesIncludeTax,
            TaxBreakdownJson = taxBreakdown,
            GrandTotal = grandLocal,
            GrandTotalUsd = grandUsd,
            WebsiteClientAddressID = address?.WebsiteClientAddressID,
            AddressSnapshot = addressSnapshot,
            ShippingMethodID = shippingMethodId,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            SalesChannel = (byte)request.SalesChannel,
            ReportToTax = reportToTax,
            MarkupTotal = markupTotalLocal,
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);

        foreach (var p in prepared)
        {
            var unitMarkup = Math.Max(0, p.ChargedLocal - p.CatalogLocal);
            var chargedUsd = ToUsd(p.ChargedLocal);

            _context.OrderItems.Add(new OrderItem
            {
                OrderID = order.OrderID,
                WebsiteID = request.WebsiteId,
                SourceWebsiteID = p.SourceWebsiteId,
                ProductVariantID = p.Variant?.ProductVariantID,
                VendorProductID = p.Vp?.VendorProductID,
                VendorID = p.Vp?.VendorID,
                TitleSnapshot = p.Title,
                SkuSnapshot = p.Sku,
                Quantity = p.Req.Quantity,
                UnitPrice = p.ChargedLocal,
                UnitPriceUsd = chargedUsd,
                UnitCostUsd = p.UnitCostUsd,
                CatalogUnitPrice = p.CatalogLocal,
                UnitMarkup = unitMarkup,
                DiscountAmount = 0,
                TotalPrice = p.ChargedLocal * p.Req.Quantity,
            });

            if (!p.TrackStock || p.Vp is null || p.Variant is null)
                continue;

            if (!await _vendorProducts.ReserveAsync(p.Vp.VendorProductID, p.Req.Quantity, ct))
            {
                await tx.RollbackAsync(ct);
                return new AdminCreateOrderResult(false, $"Insufficient vendor stock for {p.Title}.", null, null);
            }

            if (!IVendorProductService.IsUnlimited(p.Vp))
            {
                var isCrossSite = p.Vp.Vendor?.VendorType == (byte)VendorType.Site && p.SourceWebsiteId != request.WebsiteId;
                var reserveSiteId = isCrossSite ? p.SourceWebsiteId : request.WebsiteId;
                if (!isCrossSite)
                    await _vendorProducts.SyncInventoryOnHandFromListingsAsync(request.WebsiteId, p.Variant.ProductVariantID, ct);

                if (!await _inventory.ReserveAsync(reserveSiteId, p.Variant.ProductVariantID, p.Req.Quantity, ct))
                {
                    await _vendorProducts.ReleaseReservationAsync(p.Vp.VendorProductID, p.Req.Quantity, ct);
                    await tx.RollbackAsync(ct);
                    return new AdminCreateOrderResult(false, $"Insufficient stock for {p.Title}.", null, null);
                }

                if (!isCrossSite)
                {
                    var whId = await _warehouses.GetDefaultWarehouseIdAsync(request.WebsiteId, ct);
                    if (whId is int warehouseId
                        && !await _warehouses.ReserveAsync(warehouseId, p.Variant.ProductVariantID, p.Req.Quantity, ct))
                    {
                        await _inventory.ReleaseReservationAsync(reserveSiteId, p.Variant.ProductVariantID, p.Req.Quantity, ct);
                        await _vendorProducts.ReleaseReservationAsync(p.Vp.VendorProductID, p.Req.Quantity, ct);
                        await tx.RollbackAsync(ct);
                        return new AdminCreateOrderResult(false, $"Insufficient warehouse stock for {p.Title}.", null, null);
                    }
                }
            }
        }

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderID = order.OrderID,
            FromStatus = null,
            ToStatus = (byte)OrderStatus.PendingPayment,
            Note = $"Created by admin (channel: {request.SalesChannel}).",
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync(ct);
        await _vendorCredit.SettleHostOrderAsync(order.OrderID, ct);

        // Optional payment on create (card-to-card / cash / manual) — inline to avoid DI cycle with PaymentService.
        if (request.Payment is { } payReq)
        {
            var method = (PaymentMethod)payReq.Method;
            if (method is not (PaymentMethod.Manual or PaymentMethod.CashOnDelivery or PaymentMethod.BankTransfer))
            {
                await tx.RollbackAsync(ct);
                return new AdminCreateOrderResult(false, "Unsupported payment method for admin order.", null, null);
            }

            if (method == PaymentMethod.BankTransfer && payReq.BankAccountId is int baId)
            {
                var accountOk = await _context.BankAccounts.AnyAsync(
                    a => a.BankAccountID == baId && a.WebsiteID == request.WebsiteId && a.IsActive, ct);
                if (!accountOk)
                {
                    await tx.RollbackAsync(ct);
                    return new AdminCreateOrderResult(false, "Bank account not found or inactive.", null, null);
                }
            }

            if (payReq.ReceiptFileId is int fileId)
            {
                var fileOk = await _context.FileRecords.AnyAsync(
                    f => f.FileRecordID == fileId && f.WebsiteID == request.WebsiteId && !f.IsDeleted, ct);
                if (!fileOk)
                {
                    await tx.RollbackAsync(ct);
                    return new AdminCreateOrderResult(false, "Receipt file not found.", null, null);
                }
            }

            var payAmount = payReq.Amount is > 0 ? payReq.Amount.Value : grandLocal;
            var payUsd = ToUsd(payAmount);
            var markPaid = payReq.MarkAsPaid || method is not PaymentMethod.BankTransfer;
            DateTime? effectivePaidAt = null;
            if (markPaid)
            {
                effectivePaidAt = payReq.PaidAtUtc.HasValue
                    ? (payReq.PaidAtUtc.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(payReq.PaidAtUtc.Value, DateTimeKind.Utc)
                        : payReq.PaidAtUtc.Value.ToUniversalTime())
                    : DateTime.UtcNow;
            }

            _context.Payments.Add(new Payment
            {
                WebsiteID = request.WebsiteId,
                OrderID = order.OrderID,
                WebsiteClientID = client.WebsiteClientID,
                Method = (byte)method,
                BankAccountID = payReq.BankAccountId,
                ReceiptFileID = payReq.ReceiptFileId,
                Amount = payAmount,
                CurrencyCode = resolvedCurrency,
                ExchangeRateToUsd = rate,
                AmountUsd = payUsd,
                Status = (byte)(markPaid ? PaymentStatus.Paid : PaymentStatus.Pending),
                TrackingCode = string.IsNullOrWhiteSpace(payReq.Reference) ? null : payReq.Reference.Trim(),
                GatewayRefNumber = string.IsNullOrWhiteSpace(payReq.Note) ? null : payReq.Note.Trim(),
                PaidAt = effectivePaidAt,
                CreatedByMemberID = memberId,
                VerifiedByMemberID = markPaid ? memberId : null,
                CreatedAt = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync(ct);

            if (markPaid)
            {
                // Transition inside the same unit of work context (status + stock commit).
                await TransitionStatusAsync(order.OrderID, OrderStatus.Paid, memberId,
                    string.IsNullOrWhiteSpace(payReq.Note) ? $"Payment received ({method})." : payReq.Note.Trim(), ct);

                if (effectivePaidAt is DateTime at)
                {
                    var tracked = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == order.OrderID, ct);
                    if (tracked is not null)
                        tracked.PaidAt = at;
                    await _context.SaveChangesAsync(ct);
                }

                var payId = await _context.Payments.AsNoTracking()
                    .Where(p => p.OrderID == order.OrderID && p.Status == (byte)PaymentStatus.Paid)
                    .OrderByDescending(p => p.PaymentID)
                    .Select(p => (int?)p.PaymentID)
                    .FirstOrDefaultAsync(ct);
                try { await _ledger.PostOrderPaidBreakdownAsync(order.OrderID, payId, memberId, ct); }
                catch { /* ledger optional */ }
            }
        }

        await tx.CommitAsync(ct);

        await _notifications.NotifySiteAdminsAsync(
            request.WebsiteId,
            AdminNotificationType.NewOrder,
            "Admin order created",
            $"Order #{order.OrderNumber} created by admin — {order.GrandTotal:0.##} {order.CurrencyCode} ({request.SalesChannel}).",
            $"/orders/{order.OrderID}",
            order.OrderID,
            ct);

        return new AdminCreateOrderResult(true, null, order.OrderID, order.OrderNumber);
        });
    }

    private static string GenerateOrderNumber(int websiteId) =>
        $"{websiteId}-{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(100, 999)}";
}
