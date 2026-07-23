using Dotnetable.Admin.Localization;
using Dotnetable.Domain.Entities;
using MudBlazor;

namespace Dotnetable.Admin.Components.Shared;

/// <summary>Shared prerequisite checks for content admin screens (posts, categories, etc.).</summary>
public static class ContentPrerequisiteHelper
{
    public static List<DNPrerequisiteAlert.Item> ForPosts(
        IPageLocalizer L,
        IReadOnlyList<PostType> postTypes,
        int categoryCount,
        int tagCount)
    {
        var list = new List<DNPrerequisiteAlert.Item>();
        if (postTypes.Count == 0)
        {
            list.Add(new(
                L["prereq.posts.no_post_type", "No post types yet. Create at least one post type before adding posts."],
                "/content/post-types",
                L["prereq.posts.go_post_types", "Post types"]));
            return list;
        }

        if (postTypes.Any(t => t.HasCategories) && categoryCount == 0)
        {
            list.Add(new(
                L["prereq.posts.no_categories", "Some post types use categories, but none are defined yet."],
                "/content/categories",
                L["prereq.posts.go_categories", "Categories"],
                Severity.Info));
        }

        if (postTypes.Any(t => t.HasTags) && tagCount == 0)
        {
            list.Add(new(
                L["prereq.posts.no_tags", "Some post types use tags, but none are defined yet."],
                "/content/tags",
                L["prereq.posts.go_tags", "Tags"],
                Severity.Info));
        }

        return list;
    }

    public static List<DNPrerequisiteAlert.Item> ForCategories(IPageLocalizer L, int postTypeCount)
    {
        if (postTypeCount > 0) return new();
        return new List<DNPrerequisiteAlert.Item>
        {
            new(
                L["prereq.categories.no_post_type", "No post types yet. Categories are usually tied to a post type — create a post type first."],
                "/content/post-types",
                L["prereq.posts.go_post_types", "Post types"]),
        };
    }

    public static List<DNPrerequisiteAlert.Item> ForMediaStorage(IPageLocalizer L, int activeStorageCount)
    {
        if (activeStorageCount > 0) return new();
        return new List<DNPrerequisiteAlert.Item>
        {
            new(
                L["prereq.media.no_storage", "No active storage backend for this website. Configure storage before uploading files."],
                "/media/storage",
                L["prereq.media.go_storage", "Storage backends"]),
        };
    }

    public static List<DNPrerequisiteAlert.Item> ForCurrencyRates(IPageLocalizer L, int currencyCount)
    {
        if (currencyCount > 0) return new();
        return new List<DNPrerequisiteAlert.Item>
        {
            new(
                L["prereq.currency_rates.no_currencies", "No currencies are defined yet. Add currencies before configuring exchange rates."],
                "/finance/currencies",
                L["prereq.currency_rates.go_currencies", "Currencies"]),
        };
    }

    public static List<DNPrerequisiteAlert.Item> ForProducts(
        IPageLocalizer L,
        int brandCount,
        int categoryCount,
        int currencyRateCount)
    {
        var list = new List<DNPrerequisiteAlert.Item>();
        if (brandCount == 0)
        {
            list.Add(new(
                L["prereq.products.no_brands", "No brands yet. Create at least one brand before adding products."],
                "/catalog/brands",
                L["prereq.products.go_brands", "Brands"]));
        }

        if (categoryCount == 0)
        {
            list.Add(new(
                L["prereq.products.no_categories", "No product categories yet. Create a category before adding products."],
                "/catalog/categories",
                L["prereq.products.go_categories", "Categories"]));
        }

        if (currencyRateCount == 0)
        {
            list.Add(new(
                L["prereq.products.no_rates", "No exchange rate is configured for this website. Add a rate (e.g. USD → local currency) before setting product prices."],
                "/finance/currency-rates",
                L["prereq.products.go_rates", "Exchange rates"]));
        }

        return list;
    }

    public static List<DNPrerequisiteAlert.Item> ForBankAccounts(IPageLocalizer L, int bankCount)
    {
        if (bankCount > 0) return new();
        return new List<DNPrerequisiteAlert.Item>
        {
            new(
                L["prereq.bank_accounts.no_banks", "No banks are defined yet. Add banks before creating bank accounts."],
                "/finance/banks",
                L["prereq.bank_accounts.go_banks", "Banks"]),
        };
    }
}
