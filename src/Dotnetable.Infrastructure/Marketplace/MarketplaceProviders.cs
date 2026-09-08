using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;

namespace Dotnetable.Infrastructure.Marketplace;

/// <summary>Shared settings parsing and placeholder substitution for marketplace providers.</summary>
public static class MarketplaceTemplate
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Deserializes a channel's settings JSON into that provider's own settings class.</summary>
    public static T Parse<T>(MarketplaceChannelContext ctx) where T : new()
    {
        if (string.IsNullOrWhiteSpace(ctx.SettingsJson)) return new T();
        try
        {
            return JsonSerializer.Deserialize<T>(ctx.SettingsJson, JsonOptions) ?? new T();
        }
        catch (JsonException)
        {
            return new T();
        }
    }

    /// <summary>
    /// Substitutes the item placeholders in <paramref name="template"/>. Values are inserted raw —
    /// callers that build XML let <see cref="XElement"/> escape them, and the JSON body builder calls
    /// <see cref="JsonEscape"/> first.
    /// </summary>
    public static string Fill(string template, MarketplaceProductItem item, string currency,
        string inStockValue, string outOfStockValue, decimal priceDivisor, Func<string, string>? encode = null)
    {
        encode ??= static v => v;
        var divisor = priceDivisor is 0 or 1 ? 1m : priceDivisor;
        var price = item.Price / divisor;
        var compareAt = item.CompareAtPrice.HasValue ? item.CompareAtPrice.Value / divisor : (decimal?)null;

        return new StringBuilder(template)
            .Replace("{id}", encode(item.Id))
            .Replace("{sku}", encode(item.Sku ?? ""))
            .Replace("{barcode}", encode(item.Barcode ?? ""))
            .Replace("{title}", encode(item.Title))
            .Replace("{description}", encode(item.Description ?? ""))
            .Replace("{url}", encode(item.Url))
            .Replace("{image}", encode(item.ImageUrl ?? ""))
            .Replace("{price}", Money(price))
            .Replace("{compareAtPrice}", compareAt.HasValue ? Money(compareAt.Value) : "")
            .Replace("{currency}", encode(currency))
            .Replace("{availability}", encode(item.InStock ? inStockValue : outOfStockValue))
            .Replace("{quantity}", item.AvailableQuantity.ToString(CultureInfo.InvariantCulture))
            .Replace("{brand}", encode(item.Brand ?? ""))
            .Replace("{category}", encode(item.CategoryName ?? ""))
            .Replace("{remoteCategoryId}", encode(item.RemoteCategoryID ?? ""))
            .Replace("{remoteCategoryPath}", encode(item.RemoteCategoryPath ?? ""))
            .Replace("{remoteProductId}", encode(item.RemoteProductID ?? ""))
            .Replace("{sellerId}", "")
            .ToString();
    }

    /// <summary>Prices go out with no thousands separator and no trailing zeros beyond two decimals.</summary>
    public static string Money(decimal value) =>
        decimal.Round(value, 2).ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>Escapes a value for embedding inside a JSON string literal in a body template.</summary>
    public static string JsonEscape(string value)
    {
        var encoded = JsonSerializer.Serialize(value);
        return encoded[1..^1];
    }
}

/// <summary>
/// Google Merchant Center / Google Shopping. Emits the RSS 2.0 document Google's product data
/// specification defines: an <c>rss</c> root carrying the <c>http://base.google.com/ns/1.0</c>
/// namespace, one <c>channel</c>, and one <c>item</c> per variant with the required
/// <c>g:id</c>, <c>g:title</c>, <c>g:description</c>, <c>g:link</c>, <c>g:image_link</c>,
/// <c>g:availability</c>, <c>g:price</c> and <c>g:condition</c>. Bing, Facebook Catalog and several
/// other international engines accept the same document unchanged.
/// </summary>
public sealed class GoogleShoppingFeedProvider : IMarketplaceFeedProvider
{
    private static readonly XNamespace G = "http://base.google.com/ns/1.0";

    public string Key => "GoogleShopping";
    public string DisplayName => "Google Merchant Center (Google Shopping)";
    public MarketplaceIntegrationMode Mode => MarketplaceIntegrationMode.Feed;
    public bool IsIranian => false;
    public string ContentType => "application/rss+xml; charset=utf-8";

    /// <summary>A Google feed needs no credentials — the shop's own data is enough.</summary>
    public bool IsConfigured(MarketplaceChannelContext ctx) => true;

    public MarketplaceFeedResult BuildFeed(MarketplaceChannelContext ctx,
        IReadOnlyList<MarketplaceProductItem> items)
    {
        var s = MarketplaceTemplate.Parse<GoogleShoppingFeedSettings>(ctx);
        var currency = string.IsNullOrWhiteSpace(s.CurrencyCode) ? ctx.CurrencyCode : s.CurrencyCode.Trim();
        var condition = string.IsNullOrWhiteSpace(s.Condition) ? "new" : s.Condition.Trim();

        var channel = new XElement("channel",
            new XElement("title", string.IsNullOrWhiteSpace(s.ShopTitle) ? ctx.Title : s.ShopTitle),
            new XElement("link", ctx.SiteBaseUrl),
            new XElement("description", s.ShopDescription ?? ""));

        var count = 0;
        foreach (var item in items)
        {
            if (!item.InStock && !s.IncludeOutOfStock) continue;

            // Google reads g:price as the list price, so a discounted item sends the compare-at price
            // there and the actual one in g:sale_price.
            var onSale = s.IncludeSalePrice && item.CompareAtPrice > item.Price;
            var listPrice = onSale ? item.CompareAtPrice!.Value : item.Price;

            var element = new XElement("item",
                new XElement(G + "id", item.Id),
                new XElement(G + "title", Truncate(item.Title, 150)),
                new XElement(G + "description", Truncate(item.Description ?? item.Title, 5000)),
                new XElement(G + "link", item.Url),
                new XElement(G + "condition", condition),
                new XElement(G + "availability", item.InStock ? "in stock" : "out of stock"),
                new XElement(G + "price", $"{MarketplaceTemplate.Money(listPrice)} {currency}"));

            if (onSale)
                element.Add(new XElement(G + "sale_price", $"{MarketplaceTemplate.Money(item.Price)} {currency}"));

            if (!string.IsNullOrWhiteSpace(item.ImageUrl))
                element.Add(new XElement(G + "image_link", item.ImageUrl));

            foreach (var extra in item.AdditionalImageUrls.Take(10))
                element.Add(new XElement(G + "additional_image_link", extra));

            var brand = string.IsNullOrWhiteSpace(item.Brand) ? s.DefaultBrand : item.Brand;
            if (!string.IsNullOrWhiteSpace(brand))
                element.Add(new XElement(G + "brand", brand));

            if (!string.IsNullOrWhiteSpace(item.Barcode))
                element.Add(new XElement(G + "gtin", item.Barcode));
            else if (!string.IsNullOrWhiteSpace(item.Sku))
                element.Add(new XElement(G + "mpn", item.Sku));

            // Google needs identifier_exists=no when neither a GTIN nor a brand+MPN pair is present.
            if (string.IsNullOrWhiteSpace(item.Barcode) &&
                (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(item.Sku)))
                element.Add(new XElement(G + "identifier_exists", "no"));

            if (s.IncludeGoogleCategory && !string.IsNullOrWhiteSpace(item.RemoteCategoryID))
                element.Add(new XElement(G + "google_product_category", item.RemoteCategoryID));

            if (!string.IsNullOrWhiteSpace(item.RemoteCategoryPath))
                element.Add(new XElement(G + "product_type", item.RemoteCategoryPath));
            else if (!string.IsNullOrWhiteSpace(item.CategoryName))
                element.Add(new XElement(G + "product_type", item.CategoryName));

            channel.Add(element);
            count++;
        }

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("rss", new XAttribute("version", "2.0"), new XAttribute(XNamespace.Xmlns + "g", G), channel));

        return new MarketplaceFeedResult(document.Declaration + Environment.NewLine + document, ContentType, count);
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}

/// <summary>
/// Template-driven crawlable feed. Every element name and value comes from the channel settings, so
/// Torob, Emalls, Basalam's feed importer and any other engine are a configuration change rather than
/// a new class — which matters because these engines publish their schema to registered shops only.
/// </summary>
public sealed class GenericFeedProvider : IMarketplaceFeedProvider
{
    public string Key => "GenericFeed";
    public string DisplayName => "Custom product feed (Torob / Emalls / other)";
    public MarketplaceIntegrationMode Mode => MarketplaceIntegrationMode.Feed;
    public bool IsIranian => true;

    /// <summary>Set per build from the configured format; XML unless the channel asks for JSON.</summary>
    public string ContentType => "application/xml; charset=utf-8";

    public bool IsConfigured(MarketplaceChannelContext ctx)
    {
        var s = MarketplaceTemplate.Parse<GenericFeedSettings>(ctx);
        return s.FieldMap.Count > 0;
    }

    public MarketplaceFeedResult BuildFeed(MarketplaceChannelContext ctx,
        IReadOnlyList<MarketplaceProductItem> items)
    {
        var s = MarketplaceTemplate.Parse<GenericFeedSettings>(ctx);
        var currency = string.IsNullOrWhiteSpace(s.CurrencyCode) ? ctx.CurrencyCode : s.CurrencyCode.Trim();
        var selected = items.Where(i => i.InStock || s.IncludeOutOfStock);
        if (s.MaxItems > 0) selected = selected.Take(s.MaxItems);
        var list = selected.ToList();

        return string.Equals(s.Format, "Json", StringComparison.OrdinalIgnoreCase)
            ? BuildJson(s, list, currency)
            : BuildXml(s, list, currency);
    }

    private static MarketplaceFeedResult BuildXml(GenericFeedSettings s,
        IReadOnlyList<MarketplaceProductItem> items, string currency)
    {
        var root = new XElement(SafeName(s.RootElement, "products"));
        var itemName = SafeName(s.ItemElement, "product");

        foreach (var item in items)
        {
            var element = new XElement(itemName);
            foreach (var (field, template) in s.FieldMap)
            {
                var value = MarketplaceTemplate.Fill(template, item, currency,
                    s.InStockValue, s.OutOfStockValue, s.PriceDivisor);
                if (string.IsNullOrWhiteSpace(value)) continue;
                element.Add(new XElement(SafeName(field, "field"), value));
            }

            if (element.HasElements) root.Add(element);
        }

        var document = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        return new MarketplaceFeedResult(document.Declaration + Environment.NewLine + document,
            "application/xml; charset=utf-8", root.Elements().Count());
    }

    private static MarketplaceFeedResult BuildJson(GenericFeedSettings s,
        IReadOnlyList<MarketplaceProductItem> items, string currency)
    {
        var rows = new List<Dictionary<string, string>>(items.Count);
        foreach (var item in items)
        {
            var row = new Dictionary<string, string>();
            foreach (var (field, template) in s.FieldMap)
            {
                var value = MarketplaceTemplate.Fill(template, item, currency,
                    s.InStockValue, s.OutOfStockValue, s.PriceDivisor);
                if (!string.IsNullOrWhiteSpace(value)) row[field] = value;
            }

            if (row.Count > 0) rows.Add(row);
        }

        var json = JsonSerializer.Serialize(rows,
            new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        return new MarketplaceFeedResult(json, "application/json; charset=utf-8", rows.Count);
    }

    /// <summary>An admin-typed field name has to be a legal XML element name or the document will not parse.</summary>
    private static string SafeName(string? name, string fallback)
    {
        if (string.IsNullOrWhiteSpace(name)) return fallback;
        var cleaned = new string(name.Trim()
            .Select(c => char.IsLetterOrDigit(c) || c is '_' or '-' or '.' ? c : '_')
            .ToArray());
        if (cleaned.Length == 0 || (!char.IsLetter(cleaned[0]) && cleaned[0] != '_')) cleaned = "_" + cleaned;
        return cleaned;
    }
}

/// <summary>
/// Template-driven marketplace API push, for engines that take an authenticated HTTP call rather than
/// crawling a feed — Digikala Seller Center, Basalam and the like. Endpoint, auth header and body
/// shape all come from the channel settings, because each panel hands its base URL and schema to
/// approved sellers only.
/// </summary>
public sealed class GenericApiMarketplaceProvider : IMarketplaceApiProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GenericApiMarketplaceProvider(IHttpClientFactory httpClientFactory) =>
        _httpClientFactory = httpClientFactory;

    public string Key => "GenericApi";
    public string DisplayName => "Marketplace API (Digikala / Basalam / other)";
    public MarketplaceIntegrationMode Mode => MarketplaceIntegrationMode.Api;
    public bool IsIranian => true;

    public bool IsConfigured(MarketplaceChannelContext ctx)
    {
        var s = MarketplaceTemplate.Parse<GenericApiChannelSettings>(ctx);
        return !string.IsNullOrWhiteSpace(s.Url) && !string.IsNullOrWhiteSpace(s.ItemBodyTemplate);
    }

    public async Task<MarketplaceSyncResult> PushAsync(MarketplaceChannelContext ctx,
        IReadOnlyList<MarketplaceProductItem> items, CancellationToken ct = default)
    {
        var s = MarketplaceTemplate.Parse<GenericApiChannelSettings>(ctx);
        if (string.IsNullOrWhiteSpace(s.Url))
            return MarketplaceSyncResult.Fail("No endpoint is configured for this channel.");

        var payload = items.Where(i => i.InStock || s.IncludeOutOfStock).ToList();
        if (payload.Count == 0) return MarketplaceSyncResult.Ok(0, "No products matched this channel's rules.");

        var client = _httpClientFactory.CreateClient(nameof(GenericApiMarketplaceProvider));
        var batchSize = s.BatchSize < 1 ? 1 : s.BatchSize;
        int sent = 0, failed = 0;
        string? firstError = null;

        foreach (var batch in payload.Chunk(batchSize))
        {
            ct.ThrowIfCancellationRequested();

            var bodies = batch.Select(item => MarketplaceTemplate.Fill(s.ItemBodyTemplate, item, ctx.CurrencyCode,
                "true", "false", s.PriceDivisor, MarketplaceTemplate.JsonEscape));
            var body = batchSize == 1
                ? bodies.First()
                : s.BatchWrapper.Replace("{items}", string.Join(",", bodies))
                    .Replace("{sellerId}", MarketplaceTemplate.JsonEscape(s.SellerId));

            try
            {
                var response = await SendAsync(client, s, body, ct);
                var content = await response.Content.ReadAsStringAsync(ct);
                var accepted = response.IsSuccessStatusCode &&
                    (string.IsNullOrWhiteSpace(s.SuccessContains) ||
                     content.Contains(s.SuccessContains, StringComparison.OrdinalIgnoreCase));

                if (accepted)
                {
                    sent += batch.Length;
                }
                else
                {
                    failed += batch.Length;
                    firstError ??= $"HTTP {(int)response.StatusCode}: {Trim(content)}";
                }
            }
            catch (HttpRequestException ex)
            {
                failed += batch.Length;
                firstError ??= ex.Message;
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                failed += batch.Length;
                firstError ??= "The marketplace endpoint timed out.";
            }
        }

        if (failed == 0) return MarketplaceSyncResult.Ok(sent, $"{sent} products accepted.");
        if (sent == 0) return MarketplaceSyncResult.Fail(firstError ?? "Every push was rejected.");
        return MarketplaceSyncResult.Partial(sent, failed, firstError);
    }

    private static async Task<HttpResponseMessage> SendAsync(HttpClient client, GenericApiChannelSettings s,
        string body, CancellationToken ct)
    {
        var method = s.Method?.Trim().ToUpperInvariant() switch
        {
            "PUT" => HttpMethod.Put,
            "PATCH" => HttpMethod.Patch,
            _ => HttpMethod.Post,
        };

        using var request = new HttpRequestMessage(method, s.Url)
        {
            Content = new StringContent(body, Encoding.UTF8),
        };
        request.Content.Headers.ContentType =
            MediaTypeHeaderValue.Parse(string.IsNullOrWhiteSpace(s.ContentType) ? "application/json" : s.ContentType);

        foreach (var (name, template) in s.Headers)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;
            var value = (template ?? "")
                .Replace("{apiKey}", s.ApiKey)
                .Replace("{sellerId}", s.SellerId);
            request.Headers.TryAddWithoutValidation(name, value);
        }

        return await client.SendAsync(request, ct);
    }

    private static string Trim(string content) =>
        content.Length <= 300 ? content : content[..300];
}
