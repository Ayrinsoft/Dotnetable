using System.Globalization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public sealed class ProductAgentService : IProductAgentService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IProductService _products;
    private readonly IAttributeDefinitionService _attributes;
    private readonly ILanguageService _languages;

    public ProductAgentService(
        IDbContextFactory<AppDbContext> contextFactory,
        IProductService products,
        IAttributeDefinitionService attributes,
        ILanguageService languages)
    {
        _contextFactory = contextFactory;
        _products = products;
        _attributes = attributes;
        _languages = languages;
    }

    public Task<ProductAgentPrepared> PrepareAsync(int websiteId, string json, int? existingProductId, CancellationToken ct = default)
    {
        var doc = ProductAgentParser.Parse(json);
        return PrepareCoreAsync(websiteId, doc, existingProductId, ct);
    }

    public async Task<ProductAgentApplyResult> ApplyAsync(
        int websiteId, string json, int? actingMemberId, int? restrictToCreatedByMemberId, CancellationToken ct = default)
    {
        var doc = ProductAgentParser.Parse(json);
        var existing = await FindExistingAsync(websiteId, doc, ct);
        if (existing is not null && restrictToCreatedByMemberId is int owner && existing.CreatedByMemberID != owner)
            throw new ProductAgentException("This product belongs to another seller.");

        var prepared = await PrepareCoreAsync(websiteId, doc, existing?.ProductID, ct);
        var creating = existing is null;
        if (creating)
            ValidateCreate(prepared);

        var product = existing ?? new Product
        {
            WebsiteID = websiteId,
            IsActive = true,
            ProductType = (byte)ProductType.Physical,
            RequiresShipping = true,
            CreatedByMemberID = restrictToCreatedByMemberId ?? actingMemberId,
        };

        ApplyScalars(product, prepared);
        if (creating && string.IsNullOrWhiteSpace(product.Slug))
            product.Slug = Slugify(product.Title);
        if (string.IsNullOrWhiteSpace(product.Slug))
            throw new ProductAgentException("Slug is required.");
        product.Slug = Trim(product.Slug, 300)!;
        product.Title = Trim(product.Title, 300) ?? product.Title;

        await using (var context = await _contextFactory.CreateDbContextAsync(ct))
        {
            var slugTaken = await context.Products.AsNoTracking().AnyAsync(
                p => p.WebsiteID == websiteId && p.Slug == product.Slug && p.ProductID != product.ProductID, ct);
            if (slugTaken)
                throw new ProductAgentException($"Another product already uses the slug \"{product.Slug}\".");
        }

        if (prepared.Variants is not null)
            product.HasVariants = prepared.Variants.Count > 1;

        if (creating)
            product = await _products.CreateAsync(product, ct);
        else
        {
            DetachChildren(product);
            await _products.UpdateAsync(product, ct);
        }

        if (prepared.Document.Has("category") && prepared.CategoryId is int categoryId)
            await _products.SetCategoriesAsync(product.ProductID, new[] { categoryId }, categoryId, ct);

        if (prepared.Translations.Count > 0 || prepared.Document.Has("translations"))
        {
            var merged = await MergeTranslationsAsync(product.ProductID, prepared, ct);
            await _products.SetTranslationsAsync(product.ProductID, merged, ct);
        }

        if (prepared.Variants is not null)
        {
            var entities = prepared.Variants.Select(v => new ProductVariant
            {
                ProductVariantID = v.ProductVariantId,
                Sku = v.Sku,
                Title = v.Title,
                ReferencePrice = v.Price,
                CompareAtPrice = v.CompareAtPrice,
                Weight = v.Weight,
                Barcode = string.IsNullOrWhiteSpace(v.Barcode) ? null : Trim(v.Barcode, 100),
                IsDefault = v.IsDefault,
                IsActive = v.IsActive,
                ImageFileID = v.ImageFileId,
            }).ToList();
            var options = prepared.Variants
                .Select(v => (IReadOnlyList<(int, int)>)v.Options.Select(o => (o.AttributeDefinitionId, o.AttributeOptionId)).ToList())
                .ToList();
            await _products.SetVariantsAsync(product.ProductID, entities, actingMemberId, options, ct);
        }

        if (prepared.AttributeValues is not null)
        {
            var values = prepared.AttributeValues.Select(a => new ProductAttributeValue
            {
                AttributeDefinitionID = a.AttributeDefinitionId,
                AttributeOptionID = a.OptionId,
                CustomValue = a.Custom,
                NumericValue = a.Numeric,
            }).ToList();
            await _products.SetAttributeValuesAsync(product.ProductID, values, ct);
        }

        if (prepared.Warnings is not null)
        {
            var warnings = prepared.Warnings.Select(w => new ProductWarning
            {
                Severity = w.Severity,
                Text = w.Text,
                IsActive = w.IsActive,
            }).ToList();
            await _products.SetWarningsAsync(product.ProductID, warnings, ct);
        }

        if (prepared.Warranties is not null)
        {
            var rows = prepared.Warranties.Select(w => new ProductWarranty
            {
                WarrantyID = w.WarrantyId,
                CustomTitle = w.WarrantyId is null ? w.Title : null,
                CustomDescription = w.Description,
                IsActive = true,
            }).ToList();
            await _products.SetWarrantiesAsync(product.ProductID, rows, ct);
        }

        string prefix;
        await using (var context = await _contextFactory.CreateDbContextAsync(ct))
        {
            prefix = await context.Websites.AsNoTracking()
                .Where(w => w.WebsiteID == websiteId)
                .Select(w => w.ProductCodePrefix)
                .FirstAsync(ct);
        }

        return new ProductAgentApplyResult
        {
            ProductId = product.ProductID,
            ProductCode = ProductCode.Format(prefix, product.ProductID),
            Slug = product.Slug,
            Created = creating,
            Notes = prepared.Notes,
        };
    }

    public async Task<int?> FindProductIdAsync(int websiteId, string slugOrCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slugOrCode)) return null;
        var key = slugOrCode.Trim();
        var json = ProductCode.TryParse(key, out _, out _)
            ? "{\"product_code\":\"" + EscapeJson(key) + "\"}"
            : "{\"slug\":\"" + EscapeJson(key) + "\"}";
        var doc = ProductAgentParser.Parse(json);
        var existing = await FindExistingAsync(websiteId, doc, ct);
        return existing?.ProductID;
    }

    public async Task<string> ExportJsonAsync(int productId, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, ct)
            ?? throw new ProductAgentException("Product was not found.");

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var site = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == product.WebsiteID)
            .Select(w => new { w.DefaultLanguageCode, w.ProductCodePrefix })
            .FirstAsync(ct);
        var brandSlug = product.BrandID is int brandId
            ? await context.Brands.AsNoTracking().Where(b => b.BrandID == brandId).Select(b => b.Slug).FirstOrDefaultAsync(ct)
            : null;
        var categoryId = product.ProductCategoryMaps.FirstOrDefault(m => m.IsPrimary)?.ProductCategoryID
            ?? product.ProductCategoryMaps.FirstOrDefault()?.ProductCategoryID;
        var categorySlug = categoryId is int cid
            ? await context.ProductCategories.AsNoTracking().Where(c => c.ProductCategoryID == cid).Select(c => c.Slug).FirstOrDefaultAsync(ct)
            : null;

        var defIds = product.ProductVariants
            .SelectMany(v => v.VariantAttributeValues.Select(a => a.AttributeDefinitionID))
            .Distinct()
            .ToList();
        var defNames = defIds.Count == 0
            ? new Dictionary<int, string>()
            : await context.AttributeDefinitions.AsNoTracking()
                .Where(d => defIds.Contains(d.AttributeDefinitionID))
                .ToDictionaryAsync(d => d.AttributeDefinitionID, d => d.Name, ct);

        var file = new ProductAgentExportFile
        {
            Language = site.DefaultLanguageCode?.Trim(),
            ProductCode = ProductCode.Format(site.ProductCodePrefix, product.ProductID),
            Title = product.Title,
            Slug = product.Slug,
            ShortDescription = product.ShortDescription,
            ContentHtml = product.Content,
            ExpertReviewHtml = product.ExpertReview,
            ProductType = TypeName(product.ProductType),
            Status = product.Status == ProductService.PublishedStatus ? "published" : "draft",
            IsActive = product.IsActive,
            IsCatalogOnly = product.IsCatalogOnly,
            Brand = brandSlug,
            Category = categorySlug,
            SortOrder = product.SortOrder,
            DigitalDownloadUrl = product.DigitalDownloadUrl,
            DigitalServiceUrl = product.DigitalServiceUrl,
            DigitalDeliveryNote = product.DigitalDeliveryNote,
            Variants = product.ProductVariants
                .OrderByDescending(v => v.IsDefault)
                .ThenBy(v => v.ProductVariantID)
                .Select(v => new ProductAgentExportVariant
                {
                    Sku = v.Sku,
                    Title = v.Title,
                    Price = v.ReferencePrice,
                    CompareAtPrice = v.CompareAtPrice is > 0 ? v.CompareAtPrice : null,
                    Weight = v.Weight,
                    Barcode = v.Barcode,
                    IsDefault = v.IsDefault,
                    IsActive = v.IsActive,
                    Options = v.VariantAttributeValues
                        .OrderBy(a => a.AttributeDefinitionID)
                        .Select(a => new ProductAgentExportOption
                        {
                            Attribute = defNames.TryGetValue(a.AttributeDefinitionID, out var name) ? name : a.AttributeDefinitionID.ToString(CultureInfo.InvariantCulture),
                            Value = a.AttributeOption?.Value ?? "",
                            ColorHex = string.IsNullOrWhiteSpace(a.AttributeOption?.ColorHex) ? null : a.AttributeOption!.ColorHex,
                        })
                        .Where(o => !string.IsNullOrWhiteSpace(o.Value))
                        .ToList(),
                })
                .ToList(),
        };

        var translations = product.ProductTranslations
            .Where(t => !string.IsNullOrWhiteSpace(t.Title))
            .ToDictionary(
                t => t.LanguageCode,
                t => new ProductAgentExportLocale
                {
                    Title = t.Title,
                    Slug = t.Slug,
                    ShortDescription = t.ShortDescription,
                    ContentHtml = t.Content,
                    ExpertReviewHtml = t.ExpertReview,
                },
                StringComparer.OrdinalIgnoreCase);
        if (translations.Count > 0) file.Translations = translations;

        if (product.ProductAttributeValues.Count > 0)
        {
            file.Attributes = product.ProductAttributeValues
                .OrderBy(a => a.SortOrder)
                .Select(a => new ProductAgentExportAttribute
                {
                    Name = a.AttributeDefinition?.Name ?? "",
                    Value = a.AttributeOption?.Value
                        ?? (a.NumericValue is decimal n ? n.ToString(CultureInfo.InvariantCulture) : a.CustomValue),
                })
                .Where(a => !string.IsNullOrWhiteSpace(a.Name))
                .ToList();
        }

        if (product.ProductWarnings.Count > 0)
        {
            file.Warnings = product.ProductWarnings.Select(w => new ProductAgentExportWarning
            {
                Severity = w.Severity,
                Text = w.Text,
                IsActive = w.IsActive,
            }).ToList();
        }

        if (product.ProductWarranties.Count > 0)
        {
            file.Warranties = product.ProductWarranties
                .OrderBy(w => w.SortOrder)
                .Select(w => new ProductAgentExportWarranty
                {
                    Title = w.Warranty?.Title ?? w.CustomTitle,
                    Description = w.CustomDescription ?? w.Warranty?.Description,
                })
                .ToList();
        }

        return ProductAgentSample.Write(file);
    }

    private async Task<Product?> FindExistingAsync(int websiteId, ProductAgentDocument doc, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        if (doc.Has("product_code") && ProductCode.TryParse(doc.ProductCode, out _, out var productId))
        {
            var byId = await context.Products.AsNoTracking()
                .Where(p => p.ProductID == productId)
                .Select(p => new { p.ProductID, p.WebsiteID })
                .FirstOrDefaultAsync(ct);
            if (byId is not null && byId.WebsiteID != websiteId)
                throw new ProductAgentException("product_code belongs to another website.");
            if (byId is not null)
                return await _products.GetByIdAsync(byId.ProductID, ct);
        }

        if (!string.IsNullOrWhiteSpace(doc.Slug))
        {
            var id = await context.Products.AsNoTracking()
                .Where(p => p.WebsiteID == websiteId && p.Slug == doc.Slug.Trim())
                .Select(p => (int?)p.ProductID)
                .FirstOrDefaultAsync(ct);
            if (id is int found)
                return await _products.GetByIdAsync(found, ct);
        }

        return null;
    }

    private async Task<ProductAgentPrepared> PrepareCoreAsync(
        int websiteId, ProductAgentDocument doc, int? existingProductId, CancellationToken ct)
    {
        var prepared = new ProductAgentPrepared { Document = doc };
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        if (doc.Has("brand"))
        {
            if (string.IsNullOrWhiteSpace(doc.Brand))
                prepared.Notes.Add("brand is empty, so the current brand is left as it is.");
            else
            {
                var brands = await context.Brands.AsNoTracking()
                    .Include(b => b.BrandTranslations)
                    .Where(b => b.WebsiteID == websiteId)
                    .ToListAsync(ct);
                var brand = MatchNamed(brands, doc.Brand, b => b.Slug, b => b.Name, b => b.BrandTranslations.Select(t => (t.Slug, t.Name)));
                if (brand is null)
                    prepared.Notes.Add($"Brand \"{doc.Brand}\" was not found. Create it first, then import again.");
                else
                    prepared.BrandId = brand.BrandID;
            }
        }

        if (doc.Has("category"))
        {
            if (string.IsNullOrWhiteSpace(doc.Category))
                prepared.Notes.Add("category is empty, so the current category is left as it is.");
            else
            {
                var categories = await context.ProductCategories.AsNoTracking()
                    .Include(c => c.ProductCategoryTranslations)
                    .Where(c => c.WebsiteID == websiteId)
                    .ToListAsync(ct);
                var category = MatchNamed(categories, doc.Category, c => c.Slug, c => c.Name, c => c.ProductCategoryTranslations.Select(t => (t.Slug, t.Name)));
                if (category is null)
                    prepared.Notes.Add($"Category \"{doc.Category}\" was not found. Create it first, then import again.");
                else
                    prepared.CategoryId = category.ProductCategoryID;
            }
        }

        if (doc.Has("product_type"))
        {
            if (!TryParseType(doc.ProductType, out var type))
                throw new ProductAgentException("product_type must be physical, download, code, or service.");
            prepared.ProductType = type;
        }

        if (doc.Has("status"))
        {
            prepared.Status = (doc.Status ?? "").Trim().ToLowerInvariant() switch
            {
                "published" or "1" => ProductService.PublishedStatus,
                "draft" or "0" or "" => 0,
                _ => throw new ProductAgentException("status must be draft or published."),
            };
        }

        var active = await _languages.GetActiveForWebsiteAsync(websiteId, ct);
        var activeCodes = active.Select(l => l.LanguageCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var defaultCode = active.FirstOrDefault(l => l.IsDefault)?.LanguageCode ?? active.FirstOrDefault()?.LanguageCode;
        foreach (var block in doc.Translations)
        {
            if (string.IsNullOrWhiteSpace(block.Language)) continue;
            if (defaultCode is not null && block.Language.Equals(defaultCode, StringComparison.OrdinalIgnoreCase))
            {
                prepared.Notes.Add($"Translation \"{block.Language}\" is the site default. Put those fields on the root object.");
                continue;
            }
            if (!activeCodes.Contains(block.Language))
            {
                prepared.Notes.Add($"Language \"{block.Language}\" is not active on this website, so that block was skipped.");
                continue;
            }
            if (string.IsNullOrWhiteSpace(block.Title))
            {
                prepared.Notes.Add($"Translation \"{block.Language}\" has no title, so it was skipped.");
                continue;
            }
            prepared.Translations.Add((
                block.Language.Trim(),
                Trim(block.Title, 300)!,
                Trim(string.IsNullOrWhiteSpace(block.Slug) ? Slugify(block.Title) : block.Slug, 300)!,
                Trim(block.ShortDescription, 1000),
                block.ContentHtml,
                block.ExpertReviewHtml));
        }

        Dictionary<string, int>? existingBySku = null;
        Dictionary<string, int?>? imageBySku = null;
        if (existingProductId is int productId && doc.Variants is not null)
        {
            var rows = await context.ProductVariants.AsNoTracking()
                .Where(v => v.ProductID == productId)
                .Select(v => new { v.ProductVariantID, v.Sku, v.ImageFileID })
                .ToListAsync(ct);
            existingBySku = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            imageBySku = new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Sku)) continue;
                existingBySku[row.Sku.Trim()] = row.ProductVariantID;
                imageBySku[row.Sku.Trim()] = row.ImageFileID;
            }
        }

        if (doc.Variants is not null)
            prepared.Variants = await ResolveVariantsAsync(websiteId, doc.Variants, existingBySku, imageBySku, prepared, ct);

        if (doc.Attributes is not null)
            prepared.AttributeValues = await ResolveAttributesAsync(websiteId, doc.Attributes, prepared, ct);

        if (doc.Warnings is not null)
        {
            prepared.Warnings = doc.Warnings.Select(w => (
                w.Severity is "warning" or "danger" or "info" ? w.Severity : "info",
                Trim(w.Text, 1000) ?? "",
                w.IsActive)).Where(w => w.Item2.Length > 0).ToList();
        }

        if (doc.Warranties is not null)
            prepared.Warranties = await ResolveWarrantiesAsync(websiteId, doc.Warranties, context, ct);

        return prepared;
    }

    private async Task<List<ProductAgentPreparedVariant>> ResolveVariantsAsync(
        int websiteId,
        List<ProductAgentVariant> variants,
        Dictionary<string, int>? existingBySku,
        Dictionary<string, int?>? imageBySku,
        ProductAgentPrepared prepared,
        CancellationToken ct)
    {
        if (variants.Count == 0)
            throw new ProductAgentException("variants must contain at least one row.");

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var resolved = new List<ProductAgentPreparedVariant>();
        foreach (var v in variants)
        {
            var sku = (v.Sku ?? "").Trim();
            var title = (v.Title ?? "").Trim();
            if (sku.Length == 0) throw new ProductAgentException("Every variant needs a sku.");
            if (sku.Length > 100) throw new ProductAgentException($"SKU \"{sku}\" is longer than 100 characters.");
            if (title.Length == 0) throw new ProductAgentException($"Variant \"{sku}\" needs a title.");
            if (title.Length > 200) throw new ProductAgentException($"Variant \"{sku}\" title is longer than 200 characters.");
            if (v.Price <= 0) throw new ProductAgentException($"Variant \"{sku}\" price must be greater than zero.");
            if (!seen.Add(sku)) throw new ProductAgentException($"Duplicate SKU \"{sku}\".");

            var options = new List<(int, int)>();
            foreach (var option in v.Options)
            {
                var pick = await ResolveOptionAsync(websiteId, option.Attribute, option.Value, option.ColorHex, variantOnly: true, prepared, ct);
                if (pick is null) continue;
                options.RemoveAll(o => o.Item1 == pick.Value.DefId);
                options.Add((pick.Value.DefId, pick.Value.OptionId));
            }

            var id = 0;
            int? image = null;
            if (existingBySku is not null && existingBySku.TryGetValue(sku, out var existingId))
            {
                id = existingId;
                if (imageBySku is not null && imageBySku.TryGetValue(sku, out var fileId))
                    image = fileId;
            }

            resolved.Add(new ProductAgentPreparedVariant
            {
                ProductVariantId = id,
                ImageFileId = image,
                Sku = sku,
                Title = title,
                Price = v.Price,
                CompareAtPrice = v.CompareAtPrice is > 0 ? v.CompareAtPrice : null,
                Weight = v.Weight,
                Barcode = v.Barcode,
                IsDefault = v.IsDefault,
                IsActive = v.IsActive,
                Options = options,
            });
        }

        if (resolved.Count(v => v.IsDefault) != 1)
        {
            for (var i = 0; i < resolved.Count; i++)
            {
                var row = resolved[i];
                resolved[i] = new ProductAgentPreparedVariant
                {
                    ProductVariantId = row.ProductVariantId,
                    ImageFileId = row.ImageFileId,
                    Sku = row.Sku,
                    Title = row.Title,
                    Price = row.Price,
                    CompareAtPrice = row.CompareAtPrice,
                    Weight = row.Weight,
                    Barcode = row.Barcode,
                    IsDefault = i == 0,
                    IsActive = row.IsActive,
                    Options = row.Options,
                };
            }
        }

        return resolved;
    }

    private async Task<List<(int AttributeDefinitionId, int? OptionId, string? Custom, decimal? Numeric)>> ResolveAttributesAsync(
        int websiteId, List<ProductAgentAttribute> attributes, ProductAgentPrepared prepared, CancellationToken ct)
    {
        var values = new List<(int, int?, string?, decimal?)>();
        foreach (var row in attributes)
        {
            if (string.IsNullOrWhiteSpace(row.Value)) continue;
            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            var def = await FindDefinitionAsync(context, websiteId, row.Name, ct);
            if (def is null)
            {
                prepared.Notes.Add($"Attribute \"{row.Name}\" was not found, so it was skipped.");
                continue;
            }
            if (def.IsVariantAttribute)
            {
                prepared.Notes.Add($"\"{row.Name}\" is a variant option. Put it on variants[].options, not attributes.");
                continue;
            }

            if (def.InputType is 1 or 2)
            {
                var pick = await ResolveOptionAsync(websiteId, row.Name, row.Value, null, variantOnly: false, prepared, ct);
                if (pick is null) continue;
                values.Add((def.AttributeDefinitionID, pick.Value.OptionId, null, null));
            }
            else if (def.InputType == 3)
            {
                if (!decimal.TryParse(row.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
                {
                    prepared.Notes.Add($"Attribute \"{row.Name}\" expects a number. \"{row.Value}\" was skipped.");
                    continue;
                }
                values.Add((def.AttributeDefinitionID, null, null, number));
            }
            else
            {
                values.Add((def.AttributeDefinitionID, null, Trim(row.Value, 1000), null));
            }
        }
        return values;
    }

    private async Task<(int DefId, int OptionId)?> ResolveOptionAsync(
        int websiteId, string attributeName, string value, string? colorHex, bool variantOnly,
        ProductAgentPrepared prepared, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var def = await FindDefinitionAsync(context, websiteId, attributeName, ct);
        if (def is null)
        {
            prepared.Notes.Add($"Option attribute \"{attributeName}\" was not found, so \"{value}\" was skipped.");
            return null;
        }
        if (variantOnly && !def.IsVariantAttribute)
        {
            prepared.Notes.Add($"\"{attributeName}\" is not a variant attribute, so it was not applied to the variant.");
            return null;
        }
        if (variantOnly && def.InputType is not (1 or 2))
        {
            prepared.Notes.Add($"\"{attributeName}\" is not a select or color attribute, so it was skipped.");
            return null;
        }

        var option = await _attributes.EnsureOptionAsync(def.AttributeDefinitionID, value.Trim(), colorHex, ct);
        prepared.TouchedOptions.Add(new ProductAgentPreparedOption
        {
            AttributeDefinitionId = def.AttributeDefinitionID,
            AttributeOptionId = option.AttributeOptionID,
            Value = option.Value,
            ColorHex = option.ColorHex,
            IsVariantAttribute = def.IsVariantAttribute,
        });
        return (def.AttributeDefinitionID, option.AttributeOptionID);
    }

    private static async Task<AttributeDefinition?> FindDefinitionAsync(
        AppDbContext context, int websiteId, string name, CancellationToken ct)
    {
        var needle = name.Trim();
        var defs = await context.AttributeDefinitions.AsNoTracking()
            .Include(d => d.AttributeDefinitionTranslations)
            .Where(d => d.WebsiteID == websiteId && d.Active)
            .ToListAsync(ct);
        return defs.FirstOrDefault(d =>
            d.Name.Equals(needle, StringComparison.OrdinalIgnoreCase)
            || d.Code.Equals(needle, StringComparison.OrdinalIgnoreCase)
            || d.AttributeDefinitionTranslations.Any(t => t.Name.Equals(needle, StringComparison.OrdinalIgnoreCase)));
    }

    private static async Task<List<(int? WarrantyId, string? Title, string? Description)>> ResolveWarrantiesAsync(
        int websiteId, List<ProductAgentWarranty> rows, AppDbContext context, CancellationToken ct)
    {
        var catalog = await context.Warranties.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .ToListAsync(ct);
        var list = new List<(int?, string?, string?)>();
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Title) && string.IsNullOrWhiteSpace(row.Description)) continue;
            var match = string.IsNullOrWhiteSpace(row.Title)
                ? null
                : catalog.FirstOrDefault(w => w.Title.Equals(row.Title.Trim(), StringComparison.OrdinalIgnoreCase));
            list.Add((match?.WarrantyID, Trim(row.Title, 200), Trim(row.Description, 2000)));
        }
        return list;
    }

    private async Task<List<ProductTranslation>> MergeTranslationsAsync(
        int productId, ProductAgentPrepared prepared, CancellationToken ct)
    {
        var existing = await _products.GetTranslationsAsync(productId, ct);
        var byLang = existing.ToDictionary(t => t.LanguageCode, StringComparer.OrdinalIgnoreCase);
        foreach (var row in prepared.Translations)
        {
            byLang[row.Language] = new ProductTranslation
            {
                ProductID = productId,
                LanguageCode = row.Language,
                Title = row.Title,
                Slug = row.Slug,
                ShortDescription = row.Short,
                Content = row.Content,
                ExpertReview = row.Expert,
            };
        }
        return byLang.Values.ToList();
    }

    private static void ValidateCreate(ProductAgentPrepared prepared)
    {
        var doc = prepared.Document;
        if (string.IsNullOrWhiteSpace(doc.Title))
            throw new ProductAgentException("A new product needs a title.");
        if (prepared.BrandId is null)
            throw new ProductAgentException("A new product needs a brand that already exists (slug or name).");
        if (prepared.CategoryId is null)
            throw new ProductAgentException("A new product needs a category that already exists (slug or name).");
        if (prepared.Variants is null || prepared.Variants.Count == 0)
            throw new ProductAgentException("A new product needs a variants array.");
    }

    /// <summary>
    /// GetById loads the child graph. Update() on that detached graph would write every child back.
    /// The agent updates children through the dedicated Set* methods instead.
    /// </summary>
    private static void DetachChildren(Product product)
    {
        product.ProductVariants = new List<ProductVariant>();
        product.ProductTranslations = new List<ProductTranslation>();
        product.ProductCategoryMaps = new List<ProductCategoryMap>();
        product.ProductMedia = new List<ProductMedium>();
        product.ProductAttributeValues = new List<ProductAttributeValue>();
        product.ProductWarnings = new List<ProductWarning>();
        product.ProductWarranties = new List<ProductWarranty>();
        product.ProductRelationProducts = new List<ProductRelation>();
        product.ProductRelationRelatedProducts = new List<ProductRelation>();
        product.Brand = null;
        product.FeaturedImageFile = null;
        product.CreatedByMember = null;
    }

    private static void ApplyScalars(Product product, ProductAgentPrepared prepared)
    {
        var doc = prepared.Document;
        if (doc.Has("title") && !string.IsNullOrWhiteSpace(doc.Title))
            product.Title = Trim(doc.Title, 300)!;
        if (doc.Has("slug") && !string.IsNullOrWhiteSpace(doc.Slug))
            product.Slug = Trim(doc.Slug, 300)!;
        if (doc.Has("short_description"))
            product.ShortDescription = Trim(doc.ShortDescription, 1000);
        if (doc.Has("content_html"))
            product.Content = doc.ContentHtml;
        if (doc.Has("expert_review_html"))
            product.ExpertReview = doc.ExpertReviewHtml;
        if (prepared.ProductType is byte type)
            product.ProductType = type;
        if (prepared.Status is byte status)
            product.Status = status;
        if (doc.IsActive is bool active)
            product.IsActive = active;
        if (doc.IsCatalogOnly is bool catalogOnly)
            product.IsCatalogOnly = catalogOnly;
        if (doc.SortOrder is int sort)
            product.SortOrder = sort;
        if (prepared.BrandId is int brandId)
            product.BrandID = brandId;
        if (doc.Has("digital_download_url"))
            product.DigitalDownloadUrl = Trim(doc.DigitalDownloadUrl, 1000);
        if (doc.Has("digital_service_url"))
            product.DigitalServiceUrl = Trim(doc.DigitalServiceUrl, 1000);
        if (doc.Has("digital_delivery_note"))
            product.DigitalDeliveryNote = Trim(doc.DigitalDeliveryNote, 2000);
    }

    private static bool TryParseType(string? raw, out byte type)
    {
        switch ((raw ?? "").Trim().ToLowerInvariant())
        {
            case "physical" or "0":
                type = (byte)ProductType.Physical;
                return true;
            case "download" or "digital_download" or "digitaldownload" or "1":
                type = (byte)ProductType.DigitalDownload;
                return true;
            case "code" or "digital_code" or "digitalcode" or "2":
                type = (byte)ProductType.DigitalCode;
                return true;
            case "service" or "digital_service" or "digitalservice" or "3":
                type = (byte)ProductType.DigitalService;
                return true;
            default:
                type = 0;
                return false;
        }
    }

    private static string TypeName(byte type) => type switch
    {
        (byte)ProductType.DigitalDownload => "download",
        (byte)ProductType.DigitalCode => "code",
        (byte)ProductType.DigitalService => "service",
        _ => "physical",
    };

    private static T? MatchNamed<T>(
        IEnumerable<T> rows, string needle,
        Func<T, string> slug, Func<T, string> name,
        Func<T, IEnumerable<(string Slug, string Name)>> translations) where T : class
    {
        var key = needle.Trim();
        return rows.FirstOrDefault(r => slug(r).Equals(key, StringComparison.OrdinalIgnoreCase))
            ?? rows.FirstOrDefault(r => name(r).Equals(key, StringComparison.OrdinalIgnoreCase))
            ?? rows.FirstOrDefault(r => translations(r).Any(t =>
                t.Slug.Equals(key, StringComparison.OrdinalIgnoreCase) || t.Name.Equals(key, StringComparison.OrdinalIgnoreCase)));
    }

    private static string Slugify(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "product";
        var chars = new List<char>(text.Length);
        var prevDash = false;
        foreach (var c in text.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c))
            {
                chars.Add(c);
                prevDash = false;
            }
            else if (!prevDash && chars.Count > 0 && (c is ' ' or '-' or '_' || char.IsWhiteSpace(c)))
            {
                chars.Add('-');
                prevDash = true;
            }
        }
        while (chars.Count > 0 && chars[^1] == '-')
            chars.RemoveAt(chars.Count - 1);
        return chars.Count == 0 ? "product" : new string(chars.ToArray());
    }

    private static string EscapeJson(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string? Trim(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var s = value.Trim();
        return s.Length <= max ? s : s[..max];
    }
}
