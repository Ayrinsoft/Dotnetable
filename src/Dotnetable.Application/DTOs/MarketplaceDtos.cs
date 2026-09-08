namespace Dotnetable.Application.DTOs;

/// <summary>How a channel receives the catalog.</summary>
public enum MarketplaceIntegrationMode : byte
{
    /// <summary>The engine crawls a feed URL we publish. Torob, Emalls, Google Shopping.</summary>
    Feed = 0,

    /// <summary>We push the catalog to the engine's API and it answers per item. Digikala, Basalam.</summary>
    Api = 1,
}

/// <summary>Outcome recorded on <c>MarketplaceSyncLog.Status</c>.</summary>
public enum MarketplaceSyncStatus : byte
{
    Running = 0,
    Success = 1,
    Partial = 2,
    Failed = 3,
}

/// <summary>Strongly-typed view of a <c>MarketplaceChannel</c> row, handed to a provider at sync time.</summary>
public sealed class MarketplaceChannelContext
{
    public int MarketplaceChannelID { get; init; }
    public int WebsiteID { get; init; }

    /// <summary>Provider key, matching <c>IMarketplaceProvider.Key</c>.</summary>
    public string Provider { get; init; } = "";

    public string Title { get; init; } = "";

    /// <summary>Raw provider credentials/options JSON (parsed by each provider).</summary>
    public string SettingsJson { get; init; } = "{}";

    /// <summary>Absolute site root, e.g. <c>https://shop.example.com</c>, used to build product links.</summary>
    public string SiteBaseUrl { get; init; } = "";

    /// <summary>ISO currency of <see cref="MarketplaceProductItem.Price"/>, e.g. <c>IRR</c> or <c>USD</c>.</summary>
    public string CurrencyCode { get; init; } = "IRR";
}

/// <summary>
/// One sellable line offered to a channel — a product variant flattened with everything every engine
/// asks for. Providers read this and never touch the database.
/// </summary>
public sealed class MarketplaceProductItem
{
    public int ProductID { get; init; }
    public int ProductVariantID { get; init; }

    /// <summary>Stable per-item id sent to the engine. Defaults to the SKU, falling back to the variant id.</summary>
    public string Id { get; init; } = "";

    public string? Sku { get; init; }

    /// <summary>EAN/UPC/ISBN when the variant carries one, mapped from <c>ProductVariant.Barcode</c>.</summary>
    public string? Barcode { get; init; }

    public string Title { get; init; } = "";
    public string? Description { get; init; }

    /// <summary>Absolute URL of the product page.</summary>
    public string Url { get; init; } = "";

    /// <summary>Absolute URL of the main image, when the product has one.</summary>
    public string? ImageUrl { get; init; }

    public IReadOnlyList<string> AdditionalImageUrls { get; init; } = [];

    /// <summary>Current selling price in <see cref="MarketplaceChannelContext.CurrencyCode"/>.</summary>
    public decimal Price { get; init; }

    /// <summary>Pre-discount price, when higher than <see cref="Price"/>.</summary>
    public decimal? CompareAtPrice { get; init; }

    public bool InStock { get; init; }

    public int AvailableQuantity { get; init; }

    public string? Brand { get; init; }

    /// <summary>Local category name, for engines that accept free-text categories.</summary>
    public string? CategoryName { get; init; }

    /// <summary>The channel's own category id from the mapping table, when one is configured.</summary>
    public string? RemoteCategoryID { get; init; }

    /// <summary>The channel's category path from the mapping table, when one is configured.</summary>
    public string? RemoteCategoryPath { get; init; }

    /// <summary>The channel's id for this product from a previous push, when there was one.</summary>
    public string? RemoteProductID { get; init; }
}

/// <summary>Outcome of one sync run or feed build.</summary>
/// <param name="Status">Success, Partial when some items failed, Failed when nothing went through.</param>
/// <param name="ItemCount">Items accepted by the channel (or written into the feed).</param>
/// <param name="FailedCount">Items the channel rejected.</param>
/// <param name="Message">What happened, safe to show an admin.</param>
public sealed record MarketplaceSyncResult(
    MarketplaceSyncStatus Status,
    int ItemCount = 0,
    int FailedCount = 0,
    string? Message = null)
{
    public static MarketplaceSyncResult Ok(int itemCount, string? message = null) =>
        new(MarketplaceSyncStatus.Success, itemCount, 0, message);

    public static MarketplaceSyncResult Partial(int itemCount, int failedCount, string? message = null) =>
        new(MarketplaceSyncStatus.Partial, itemCount, failedCount, message);

    public static MarketplaceSyncResult Fail(string message) =>
        new(MarketplaceSyncStatus.Failed, 0, 0, message);
}

/// <summary>A built feed, ready to be returned from the public feed endpoint.</summary>
/// <param name="Content">The serialized document.</param>
/// <param name="ContentType">MIME type to serve it under.</param>
/// <param name="ItemCount">How many items it carries.</param>
public sealed record MarketplaceFeedResult(string Content, string ContentType, int ItemCount);

/// <summary>A registered channel as shown in the admin list.</summary>
public sealed class MarketplaceChannelInfo
{
    public int MarketplaceChannelID { get; init; }
    public int WebsiteID { get; init; }
    public string Provider { get; init; } = "";
    public string ProviderDisplayName { get; init; } = "";
    public MarketplaceIntegrationMode Mode { get; init; }
    public string Title { get; init; } = "";
    public string SettingsJSON { get; init; } = "{}";
    public string FeedToken { get; init; } = "";
    public bool IncludeAllProducts { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public int SyncIntervalMinutes { get; init; }
    public DateTime? LastSyncAt { get; init; }
    public MarketplaceSyncStatus? LastSyncStatus { get; init; }
    public string? LastSyncMessage { get; init; }

    /// <summary>False when the stored JSON is missing something the provider cannot run without.</summary>
    public bool IsConfigured { get; init; }

    /// <summary>Public feed URL for feed-mode channels, null for API-mode ones.</summary>
    public string? FeedUrl { get; init; }

    public DateTime CreatedAt { get; init; }
}

/// <summary>Create/update payload for a channel registration.</summary>
public sealed class MarketplaceChannelInput
{
    public int WebsiteID { get; set; }
    public string Provider { get; set; } = "";
    public string Title { get; set; } = "";
    public string SettingsJSON { get; set; } = "{}";
    public bool IncludeAllProducts { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public int SyncIntervalMinutes { get; set; }
}

/// <summary>One row of the per-channel category rules, as edited in the admin mapping grid.</summary>
public sealed class MarketplaceCategoryMapInfo
{
    public int ProductCategoryID { get; set; }
    public string CategoryName { get; set; } = "";
    public bool IsIncluded { get; set; }
    public string? RemoteCategoryID { get; set; }
    public string? RemoteCategoryPath { get; set; }
}

/// <summary>One past run, as shown in the admin log table.</summary>
public sealed class MarketplaceSyncLogInfo
{
    public int MarketplaceSyncLogID { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? FinishedAt { get; init; }
    public MarketplaceSyncStatus Status { get; init; }
    public string TriggeredBy { get; init; } = "";
    public int ItemCount { get; init; }
    public int FailedCount { get; init; }
    public string? Message { get; init; }
}

// ── Per-provider settings, persisted in MarketplaceChannel.SettingsJSON ──────────────────

/// <summary>
/// Google Merchant Center / Google Shopping. Emits the RSS 2.0 document Google specifies
/// (<c>http://base.google.com/ns/1.0</c> namespace), which several other international engines
/// accept unchanged.
/// </summary>
public sealed class GoogleShoppingFeedSettings
{
    /// <summary>Shown as the feed's channel title. Defaults to the website name when blank.</summary>
    public string ShopTitle { get; set; } = "";

    public string ShopDescription { get; set; } = "";

    /// <summary>Value for <c>g:condition</c>: new, refurbished or used.</summary>
    public string Condition { get; set; } = "new";

    /// <summary>Currency sent alongside the price, e.g. <c>IRR</c>. Blank uses the website currency.</summary>
    public string CurrencyCode { get; set; } = "";

    /// <summary>Optional <c>g:brand</c> used when a product has no brand of its own.</summary>
    public string DefaultBrand { get; set; } = "";

    /// <summary>Emit <c>g:sale_price</c> when a compare-at price is present.</summary>
    public bool IncludeSalePrice { get; set; } = true;

    /// <summary>Emit <c>g:google_product_category</c> from the category mapping.</summary>
    public bool IncludeGoogleCategory { get; set; } = true;

    /// <summary>Keep out-of-stock items in the feed (marked <c>out of stock</c>) instead of dropping them.</summary>
    public bool IncludeOutOfStock { get; set; } = true;
}

/// <summary>
/// Template-driven XML/JSON feed: covers Torob, Emalls and any other engine that crawls a document,
/// without writing a provider class per engine. Every tag name is configurable because these engines
/// publish their schema to registered shops only — fill these in from the spec they hand you.
/// </summary>
public sealed class GenericFeedSettings
{
    /// <summary>Document format: <c>Xml</c> or <c>Json</c>.</summary>
    public string Format { get; set; } = "Xml";

    /// <summary>XML root element name.</summary>
    public string RootElement { get; set; } = "products";

    /// <summary>XML element wrapping each item.</summary>
    public string ItemElement { get; set; } = "product";

    /// <summary>
    /// Field name → value template. Templates take the placeholders <c>{id}</c>, <c>{sku}</c>,
    /// <c>{barcode}</c>, <c>{title}</c>, <c>{description}</c>, <c>{url}</c>, <c>{image}</c>,
    /// <c>{price}</c>, <c>{compareAtPrice}</c>, <c>{currency}</c>, <c>{availability}</c>,
    /// <c>{quantity}</c>, <c>{brand}</c>, <c>{category}</c>, <c>{remoteCategoryId}</c> and
    /// <c>{remoteCategoryPath}</c>. A field whose template resolves to empty is omitted.
    /// </summary>
    public Dictionary<string, string> FieldMap { get; set; } = new()
    {
        ["product_id"] = "{id}",
        ["title"] = "{title}",
        ["page_url"] = "{url}",
        ["image_link"] = "{image}",
        ["price"] = "{price}",
        ["availability"] = "{availability}",
        ["category_name"] = "{category}",
    };

    /// <summary>Text emitted for <c>{availability}</c> when the item is in stock.</summary>
    public string InStockValue { get; set; } = "instock";

    /// <summary>Text emitted for <c>{availability}</c> when it is not.</summary>
    public string OutOfStockValue { get; set; } = "outofstock";

    /// <summary>Currency written into <c>{currency}</c>. Blank uses the website currency.</summary>
    public string CurrencyCode { get; set; } = "";

    /// <summary>
    /// Divisor applied to the price before it is written, for engines that bill in another unit —
    /// 10 turns Rial into Toman. 0 or 1 leaves the price untouched.
    /// </summary>
    public decimal PriceDivisor { get; set; } = 1;

    /// <summary>Keep out-of-stock items in the document instead of dropping them.</summary>
    public bool IncludeOutOfStock { get; set; } = true;

    /// <summary>Cap on how many items the document carries. 0 means no cap.</summary>
    public int MaxItems { get; set; }
}

/// <summary>
/// Template-driven marketplace API push: covers Digikala Seller Center, Basalam and anything else
/// that takes an authenticated HTTP call per item or per batch. The endpoint, auth header and body
/// template are configuration rather than code, because each panel hands its own spec and base URL to
/// approved sellers only.
/// </summary>
public sealed class GenericApiChannelSettings
{
    /// <summary>Absolute endpoint the catalog is posted to.</summary>
    public string Url { get; set; } = "";

    /// <summary>POST, PUT or PATCH.</summary>
    public string Method { get; set; } = "POST";

    /// <summary>Bearer token / API key, substituted into headers as <c>{apiKey}</c>.</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>Seller/vendor identifier, substituted as <c>{sellerId}</c>.</summary>
    public string SellerId { get; set; } = "";

    /// <summary>Extra headers. Values take <c>{apiKey}</c> and <c>{sellerId}</c>.</summary>
    public Dictionary<string, string> Headers { get; set; } = new()
    {
        ["Authorization"] = "Bearer {apiKey}",
    };

    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Body template for one item, taking the same placeholders as <see cref="GenericFeedSettings.FieldMap"/>.
    /// When <see cref="BatchSize"/> is above 1 these are joined into a JSON array under
    /// <see cref="BatchWrapper"/>.
    /// </summary>
    public string ItemBodyTemplate { get; set; } =
        """{"id":"{id}","title":"{title}","price":{price},"stock":{quantity},"url":"{url}"}""";

    /// <summary>Items per request. 1 posts each item on its own.</summary>
    public int BatchSize { get; set; } = 1;

    /// <summary>
    /// Envelope for a batch, taking <c>{items}</c> (the joined item bodies) and <c>{sellerId}</c>.
    /// Ignored when <see cref="BatchSize"/> is 1.
    /// </summary>
    public string BatchWrapper { get; set; } = """{"products":[{items}]}""";

    /// <summary>Substring that must appear in a 2xx response for the push to count as accepted.</summary>
    public string? SuccessContains { get; set; }

    /// <summary>Divisor applied to the price, e.g. 10 to bill in Toman while the shop prices in Rial.</summary>
    public decimal PriceDivisor { get; set; } = 1;

    /// <summary>Push out-of-stock items (with a zero quantity) instead of skipping them.</summary>
    public bool IncludeOutOfStock { get; set; } = true;
}
