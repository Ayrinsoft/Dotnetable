using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

[Authorize(Policy = RoleKeys.ClientProfile)]
public class WishlistController : BaseController
{
    private readonly IWishlistService _wishlist;
    private readonly IWebsiteService _websiteService;
    private readonly ICurrencyConversionService _currency;

    public WishlistController(IWishlistService wishlist, IWebsiteService websiteService, ICurrencyConversionService currency)
    {
        _wishlist = wishlist;
        _websiteService = websiteService;
        _currency = currency;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? currency = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        var websiteId = website?.WebsiteID ?? 0;
        var storeUsd = websiteId > 0 && await _currency.GetStorePricesInUsdAsync(websiteId, ct);

        var items = await _wishlist.GetItemsAsync(CurrentClientId, ct);
        var result = new List<object>();
        foreach (var i in items)
        {
            var v = i.ProductVariant;
            MoneyDto price;
            if (websiteId > 0)
            {
                var local = v.ReferencePrice > 0 ? v.ReferencePrice : v.ReferencePriceUsd;
                price = await _currency.ToDisplayFromLocalAsync(
                    websiteId, local, null, storeUsd ? currency : null,
                    v.ReferencePriceUsd > 0 ? v.ReferencePriceUsd : null, ct);
            }
            else
            {
                price = new MoneyDto
                {
                    Amount = v.ReferencePrice > 0 ? v.ReferencePrice : v.ReferencePriceUsd,
                    AmountUsd = v.ReferencePriceUsd,
                    CurrencyCode = "USD",
                };
            }

            result.Add(new
            {
                i.WishlistItemID,
                i.ProductVariantID,
                ProductID = v.ProductID,
                Title = v.Product.Title,
                Sku = v.Sku,
                ImageUrl = v.ImageFile?.ThumbnailCDN ?? v.ImageFile?.CNDUrl,
                price,
                // Backward-compatible.
                PriceUsd = price.AmountUsd,
            });
        }

        return Ok(result);
    }

    public sealed record AddItemRequest(int VariantId);

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        await _wishlist.AddItemAsync(website.WebsiteID, CurrentClientId, request.VariantId, ct);
        return Ok();
    }

    [HttpDelete("items/{variantId:int}")]
    public async Task<IActionResult> RemoveItem(int variantId, CancellationToken ct = default)
    {
        await _wishlist.RemoveItemAsync(CurrentClientId, variantId, ct);
        return Ok();
    }

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");
}
