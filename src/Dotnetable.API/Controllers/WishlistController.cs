using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

[Authorize(Policy = RoleKeys.ClientProfile)]
public class WishlistController : BaseController
{
    private readonly IWishlistService _wishlist;
    private readonly IWebsiteService _websiteService;

    public WishlistController(IWishlistService wishlist, IWebsiteService websiteService)
    {
        _wishlist = wishlist;
        _websiteService = websiteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var items = await _wishlist.GetItemsAsync(CurrentClientId, ct);
        return Ok(items.Select(i => new
        {
            i.WishlistItemID,
            i.ProductVariantID,
            ProductID = i.ProductVariant.ProductID,
            Title = i.ProductVariant.Product.Title,
            Sku = i.ProductVariant.Sku,
            ImageUrl = i.ProductVariant.ImageFile?.ThumbnailCDN ?? i.ProductVariant.ImageFile?.CNDUrl,
            PriceUsd = i.ProductVariant.OverridePrice ?? i.ProductVariant.ReferencePriceUsd,
        }));
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
