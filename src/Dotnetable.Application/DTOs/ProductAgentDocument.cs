using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dotnetable.Application.DTOs;

/// <summary>Thrown when a product agent JSON file cannot be applied.</summary>
public sealed class ProductAgentException : Exception
{
    public ProductAgentException(string message) : base(message) { }
}

/// <summary>One language block inside a product agent file. The root uses the same fields.</summary>
public sealed class ProductAgentLocale
{
    public string Language { get; set; } = "";
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? ContentHtml { get; set; }
    public string? ExpertReviewHtml { get; set; }
}

public sealed class ProductAgentOption
{
    public string Attribute { get; set; } = "";
    public string Value { get; set; } = "";
    public string? ColorHex { get; set; }
}

public sealed class ProductAgentVariant
{
    public string Sku { get; set; } = "";
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? Weight { get; set; }
    public string? Barcode { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public List<ProductAgentOption> Options { get; set; } = new();
}

public sealed class ProductAgentWarning
{
    public string Severity { get; set; } = "info";
    public string Text { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class ProductAgentWarranty
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public sealed class ProductAgentAttribute
{
    public string Name { get; set; } = "";
    public string? Value { get; set; }
}

/// <summary>
/// Canonical product snapshot for file import and <c>POST /api/product-agent</c>.
/// <see cref="Present"/> lists JSON keys that were sent (snake_case). Omitted keys are left unchanged on update.
/// </summary>
public sealed class ProductAgentDocument
{
    public HashSet<string> Present { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool Has(string name) => Present.Contains(name);

    public string? Language { get; set; }
    public string? ProductCode { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? ContentHtml { get; set; }
    public string? ExpertReviewHtml { get; set; }
    public string? ProductType { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsCatalogOnly { get; set; }
    public int? SortOrder { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? DigitalDownloadUrl { get; set; }
    public string? DigitalServiceUrl { get; set; }
    public string? DigitalDeliveryNote { get; set; }
    public List<ProductAgentLocale> Translations { get; set; } = new();
    public List<ProductAgentVariant>? Variants { get; set; }
    public List<ProductAgentWarning>? Warnings { get; set; }
    public List<ProductAgentWarranty>? Warranties { get; set; }
    public List<ProductAgentAttribute>? Attributes { get; set; }
}

public static class ProductAgentParser
{
    public const long MaxBytes = 2 * 1024 * 1024;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static ProductAgentDocument Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ProductAgentException("The JSON file is empty.");

        JsonElement root;
        try
        {
            root = JsonSerializer.Deserialize<JsonElement>(json, Options);
        }
        catch (JsonException ex)
        {
            throw new ProductAgentException("The file is not valid JSON. " + ex.Message);
        }

        if (root.ValueKind != JsonValueKind.Object)
            throw new ProductAgentException("The JSON root must be an object.");

        var doc = new ProductAgentDocument();
        foreach (var prop in root.EnumerateObject())
            doc.Present.Add(prop.Name);

        doc.Language = Str(root, "language");
        doc.ProductCode = First(Str(root, "product_code"), Str(root, "productCode"));
        doc.Title = Str(root, "title");
        doc.Slug = Str(root, "slug");
        doc.ShortDescription = First(Str(root, "short_description"), Str(root, "shortDescription"), Str(root, "summary"));
        doc.ContentHtml = First(Str(root, "content_html"), Str(root, "contentHtml"), Str(root, "content"));
        doc.ExpertReviewHtml = First(Str(root, "expert_review_html"), Str(root, "expertReviewHtml"), Str(root, "expert_review"));
        doc.ProductType = First(Str(root, "product_type"), Str(root, "productType"));
        doc.Status = Str(root, "status");
        doc.IsActive = Bool(root, "is_active") ?? Bool(root, "isActive");
        doc.IsCatalogOnly = Bool(root, "is_catalog_only") ?? Bool(root, "isCatalogOnly");
        doc.SortOrder = Int(root, "sort_order") ?? Int(root, "sortOrder");
        doc.Brand = Str(root, "brand");
        doc.Category = Str(root, "category");
        doc.DigitalDownloadUrl = First(Str(root, "digital_download_url"), Str(root, "digitalDownloadUrl"));
        doc.DigitalServiceUrl = First(Str(root, "digital_service_url"), Str(root, "digitalServiceUrl"));
        doc.DigitalDeliveryNote = First(Str(root, "digital_delivery_note"), Str(root, "digitalDeliveryNote"));

        if (Has(root, "translations"))
            doc.Translations = ReadTranslations(root.GetProperty("translations"));
        if (Has(root, "variants"))
            doc.Variants = ReadVariants(Prop(root, "variants"));
        if (Has(root, "warnings"))
            doc.Warnings = ReadWarnings(Prop(root, "warnings"));
        if (Has(root, "warranties"))
            doc.Warranties = ReadWarranties(Prop(root, "warranties"));
        if (Has(root, "attributes"))
            doc.Attributes = ReadAttributes(Prop(root, "attributes"));

        // Aliases count as the canonical key so apply logic stays on one name.
        if (doc.Has("productCode")) doc.Present.Add("product_code");
        if (doc.Has("shortDescription") || doc.Has("summary")) doc.Present.Add("short_description");
        if (doc.Has("contentHtml") || doc.Has("content")) doc.Present.Add("content_html");
        if (doc.Has("expertReviewHtml") || doc.Has("expert_review")) doc.Present.Add("expert_review_html");
        if (doc.Has("productType")) doc.Present.Add("product_type");
        if (doc.Has("isActive")) doc.Present.Add("is_active");
        if (doc.Has("isCatalogOnly")) doc.Present.Add("is_catalog_only");
        if (doc.Has("sortOrder")) doc.Present.Add("sort_order");
        if (doc.Has("digitalDownloadUrl")) doc.Present.Add("digital_download_url");
        if (doc.Has("digitalServiceUrl")) doc.Present.Add("digital_service_url");
        if (doc.Has("digitalDeliveryNote")) doc.Present.Add("digital_delivery_note");

        if (!doc.Has("title") && doc.Variants is null && !doc.Has("slug") && !doc.Has("product_code"))
            throw new ProductAgentException("The file has no title, slug, product_code, or variants.");

        return doc;
    }

    private static List<ProductAgentLocale> ReadTranslations(JsonElement node)
    {
        var list = new List<ProductAgentLocale>();
        if (node.ValueKind != JsonValueKind.Object) return list;
        foreach (var prop in node.EnumerateObject())
        {
            if (prop.Value.ValueKind != JsonValueKind.Object) continue;
            var block = prop.Value;
            list.Add(new ProductAgentLocale
            {
                Language = prop.Name.Trim(),
                Title = Str(block, "title"),
                Slug = Str(block, "slug"),
                ShortDescription = First(Str(block, "short_description"), Str(block, "shortDescription"), Str(block, "summary")),
                ContentHtml = First(Str(block, "content_html"), Str(block, "contentHtml"), Str(block, "content")),
                ExpertReviewHtml = First(Str(block, "expert_review_html"), Str(block, "expertReviewHtml"), Str(block, "expert_review")),
            });
        }
        return list;
    }

    private static List<ProductAgentVariant> ReadVariants(JsonElement node)
    {
        if (node.ValueKind != JsonValueKind.Array)
            throw new ProductAgentException("variants must be an array.");
        var list = new List<ProductAgentVariant>();
        foreach (var item in node.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                throw new ProductAgentException("Each variant must be an object.");
            var price = Dec(item, "price") ?? Dec(item, "reference_price") ?? 0;
            list.Add(new ProductAgentVariant
            {
                Sku = Str(item, "sku") ?? "",
                Title = Str(item, "title") ?? "",
                Price = price,
                CompareAtPrice = Dec(item, "compare_at_price") ?? Dec(item, "compareAtPrice"),
                Weight = Dec(item, "weight"),
                Barcode = Str(item, "barcode"),
                IsDefault = Bool(item, "is_default") ?? Bool(item, "isDefault") ?? false,
                IsActive = Bool(item, "is_active") ?? Bool(item, "isActive") ?? true,
                Options = ReadOptions(item),
            });
        }
        return list;
    }

    private static List<ProductAgentOption> ReadOptions(JsonElement variant)
    {
        if (!variant.TryGetProperty("options", out var node) && !variant.TryGetProperty("Options", out node))
            return new List<ProductAgentOption>();
        var list = new List<ProductAgentOption>();
        if (node.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in node.EnumerateObject())
            {
                var value = prop.Value.ValueKind == JsonValueKind.String
                    ? prop.Value.GetString() ?? ""
                    : prop.Value.ValueKind == JsonValueKind.Null ? "" : prop.Value.ToString();
                if (string.IsNullOrWhiteSpace(prop.Name) || string.IsNullOrWhiteSpace(value)) continue;
                list.Add(new ProductAgentOption { Attribute = prop.Name.Trim(), Value = value.Trim() });
            }
            return list;
        }
        if (node.ValueKind != JsonValueKind.Array) return list;
        foreach (var item in node.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;
            var attribute = First(Str(item, "attribute"), Str(item, "name")) ?? "";
            var value = Str(item, "value") ?? "";
            if (string.IsNullOrWhiteSpace(attribute) || string.IsNullOrWhiteSpace(value)) continue;
            list.Add(new ProductAgentOption
            {
                Attribute = attribute.Trim(),
                Value = value.Trim(),
                ColorHex = First(Str(item, "color_hex"), Str(item, "colorHex")),
            });
        }
        return list;
    }

    private static List<ProductAgentWarning> ReadWarnings(JsonElement node)
    {
        if (node.ValueKind != JsonValueKind.Array)
            throw new ProductAgentException("warnings must be an array.");
        var list = new List<ProductAgentWarning>();
        foreach (var item in node.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;
            var text = Str(item, "text") ?? "";
            if (string.IsNullOrWhiteSpace(text)) continue;
            list.Add(new ProductAgentWarning
            {
                Severity = (Str(item, "severity") ?? "info").Trim().ToLowerInvariant(),
                Text = text.Trim(),
                IsActive = Bool(item, "is_active") ?? Bool(item, "isActive") ?? true,
            });
        }
        return list;
    }

    private static List<ProductAgentWarranty> ReadWarranties(JsonElement node)
    {
        if (node.ValueKind != JsonValueKind.Array)
            throw new ProductAgentException("warranties must be an array.");
        var list = new List<ProductAgentWarranty>();
        foreach (var item in node.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;
            list.Add(new ProductAgentWarranty
            {
                Title = First(Str(item, "title"), Str(item, "custom_title")),
                Description = First(Str(item, "description"), Str(item, "custom_description")),
            });
        }
        return list;
    }

    private static List<ProductAgentAttribute> ReadAttributes(JsonElement node)
    {
        if (node.ValueKind != JsonValueKind.Array)
            throw new ProductAgentException("attributes must be an array.");
        var list = new List<ProductAgentAttribute>();
        foreach (var item in node.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;
            var name = First(Str(item, "name"), Str(item, "attribute")) ?? "";
            if (string.IsNullOrWhiteSpace(name)) continue;
            list.Add(new ProductAgentAttribute { Name = name.Trim(), Value = Str(item, "value") });
        }
        return list;
    }

    private static bool Has(JsonElement obj, string name) =>
        obj.TryGetProperty(name, out _) || obj.EnumerateObject().Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    private static JsonElement Prop(JsonElement obj, string name)
    {
        if (obj.TryGetProperty(name, out var direct)) return direct;
        foreach (var p in obj.EnumerateObject())
            if (p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return p.Value;
        return default;
    }

    private static string? Str(JsonElement obj, string name)
    {
        var el = Prop(obj, name);
        if (el.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null) return null;
        if (el.ValueKind == JsonValueKind.String) return el.GetString();
        return el.ToString();
    }

    private static bool? Bool(JsonElement obj, string name)
    {
        var el = Prop(obj, name);
        if (el.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null) return null;
        if (el.ValueKind == JsonValueKind.True) return true;
        if (el.ValueKind == JsonValueKind.False) return false;
        if (el.ValueKind == JsonValueKind.String && bool.TryParse(el.GetString(), out var b)) return b;
        return null;
    }

    private static int? Int(JsonElement obj, string name)
    {
        var el = Prop(obj, name);
        if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var n)) return n;
        if (el.ValueKind == JsonValueKind.String && int.TryParse(el.GetString(), out n)) return n;
        return null;
    }

    private static decimal? Dec(JsonElement obj, string name)
    {
        var el = Prop(obj, name);
        if (el.ValueKind == JsonValueKind.Number && el.TryGetDecimal(out var n)) return n;
        if (el.ValueKind == JsonValueKind.String && decimal.TryParse(el.GetString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out n))
            return n;
        return null;
    }

    private static string? First(params string?[] values)
    {
        foreach (var v in values)
            if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
        return null;
    }
}

/// <summary>Shape agents should emit. Shown on the products page.</summary>
public static class ProductAgentSample
{
    public const string Json = """
        {
          "language": "fa",
          "product_code": "DN-42",
          "title": "کفش پیاده‌روی",
          "slug": "walking-shoe",
          "short_description": "خلاصهٔ کوتاه برای فهرست.",
          "content_html": "<p>توضیح کامل کالا.</p>",
          "expert_review_html": "<p>بررسی تخصصی.</p>",
          "product_type": "physical",
          "status": "draft",
          "is_active": true,
          "is_catalog_only": false,
          "brand": "brand-slug",
          "category": "category-slug",
          "sort_order": 0,
          "translations": {
            "en": {
              "title": "Walking shoe",
              "slug": "walking-shoe-en",
              "short_description": "Short summary.",
              "content_html": "<p>Full description.</p>",
              "expert_review_html": "<p>Expert review.</p>"
            }
          },
          "variants": [
            {
              "sku": "SHOE-RED-42",
              "title": "قرمز / ۴۲",
              "price": 1200000,
              "compare_at_price": 1500000,
              "weight": 0.8,
              "barcode": "",
              "is_default": true,
              "is_active": true,
              "options": [
                { "attribute": "Color", "value": "Red", "color_hex": "#c0392b" },
                { "attribute": "Size", "value": "42" }
              ]
            }
          ],
          "attributes": [
            { "name": "Material", "value": "Mesh" }
          ],
          "warnings": [
            { "severity": "info", "text": "سایز را قبل از خرید اندازه بگیرید.", "is_active": true }
          ],
          "warranties": [
            { "title": "۱۸ ماه گارانتی", "description": "گارانتی شرکت." }
          ]
        }
        """;

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string Write(ProductAgentExportFile file) =>
        JsonSerializer.Serialize(file, WriteOptions);
}

/// <summary>File written by export. Property names match <see cref="ProductAgentSample"/>.</summary>
public sealed class ProductAgentExportFile
{
    [JsonPropertyName("language")] public string? Language { get; set; }
    [JsonPropertyName("product_code")] public string? ProductCode { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("slug")] public string? Slug { get; set; }
    [JsonPropertyName("short_description")] public string? ShortDescription { get; set; }
    [JsonPropertyName("content_html")] public string? ContentHtml { get; set; }
    [JsonPropertyName("expert_review_html")] public string? ExpertReviewHtml { get; set; }
    [JsonPropertyName("product_type")] public string? ProductType { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
    [JsonPropertyName("is_catalog_only")] public bool IsCatalogOnly { get; set; }
    [JsonPropertyName("brand")] public string? Brand { get; set; }
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("sort_order")] public int SortOrder { get; set; }
    [JsonPropertyName("digital_download_url")] public string? DigitalDownloadUrl { get; set; }
    [JsonPropertyName("digital_service_url")] public string? DigitalServiceUrl { get; set; }
    [JsonPropertyName("digital_delivery_note")] public string? DigitalDeliveryNote { get; set; }
    [JsonPropertyName("translations")] public Dictionary<string, ProductAgentExportLocale>? Translations { get; set; }
    [JsonPropertyName("variants")] public List<ProductAgentExportVariant> Variants { get; set; } = new();
    [JsonPropertyName("attributes")] public List<ProductAgentExportAttribute>? Attributes { get; set; }
    [JsonPropertyName("warnings")] public List<ProductAgentExportWarning>? Warnings { get; set; }
    [JsonPropertyName("warranties")] public List<ProductAgentExportWarranty>? Warranties { get; set; }
}

public sealed class ProductAgentExportLocale
{
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("slug")] public string? Slug { get; set; }
    [JsonPropertyName("short_description")] public string? ShortDescription { get; set; }
    [JsonPropertyName("content_html")] public string? ContentHtml { get; set; }
    [JsonPropertyName("expert_review_html")] public string? ExpertReviewHtml { get; set; }
}

public sealed class ProductAgentExportVariant
{
    [JsonPropertyName("sku")] public string Sku { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("compare_at_price")] public decimal? CompareAtPrice { get; set; }
    [JsonPropertyName("weight")] public decimal? Weight { get; set; }
    [JsonPropertyName("barcode")] public string? Barcode { get; set; }
    [JsonPropertyName("is_default")] public bool IsDefault { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
    [JsonPropertyName("options")] public List<ProductAgentExportOption> Options { get; set; } = new();
}

public sealed class ProductAgentExportOption
{
    [JsonPropertyName("attribute")] public string Attribute { get; set; } = "";
    [JsonPropertyName("value")] public string Value { get; set; } = "";
    [JsonPropertyName("color_hex")] public string? ColorHex { get; set; }
}

public sealed class ProductAgentExportAttribute
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("value")] public string? Value { get; set; }
}

public sealed class ProductAgentExportWarning
{
    [JsonPropertyName("severity")] public string Severity { get; set; } = "info";
    [JsonPropertyName("text")] public string Text { get; set; } = "";
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
}

public sealed class ProductAgentExportWarranty
{
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
}

public sealed class ProductAgentPreparedOption
{
    public int AttributeDefinitionId { get; init; }
    public int AttributeOptionId { get; init; }
    public string Value { get; init; } = "";
    public string? ColorHex { get; init; }
    public bool IsVariantAttribute { get; init; }
}

public sealed class ProductAgentPreparedVariant
{
    public int ProductVariantId { get; init; }
    public int? ImageFileId { get; init; }
    public string Sku { get; init; } = "";
    public string Title { get; init; } = "";
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public decimal? Weight { get; init; }
    public string? Barcode { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public List<(int AttributeDefinitionId, int AttributeOptionId)> Options { get; init; } = new();
}

/// <summary>Resolved document ready to drop onto the edit form or to persist.</summary>
public sealed class ProductAgentPrepared
{
    public ProductAgentDocument Document { get; init; } = new();
    public List<string> Notes { get; } = new();
    public int? BrandId { get; set; }
    public int? CategoryId { get; set; }
    public byte? ProductType { get; set; }
    public byte? Status { get; set; }
    public List<ProductAgentPreparedOption> TouchedOptions { get; } = new();
    public List<ProductAgentPreparedVariant>? Variants { get; set; }
    public List<(int AttributeDefinitionId, int? OptionId, string? Custom, decimal? Numeric)>? AttributeValues { get; set; }
    public List<(string Severity, string Text, bool IsActive)>? Warnings { get; set; }
    public List<(int? WarrantyId, string? Title, string? Description)>? Warranties { get; set; }
    public List<(string Language, string Title, string Slug, string? Short, string? Content, string? Expert)> Translations { get; } = new();
}

public sealed class ProductAgentApplyResult
{
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = "";
    public string Slug { get; init; } = "";
    public bool Created { get; init; }
    public IReadOnlyList<string> Notes { get; init; } = Array.Empty<string>();
}
