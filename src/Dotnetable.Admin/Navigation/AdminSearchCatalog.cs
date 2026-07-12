using Dotnetable.Application.Authorization;
using MudBlazor;

namespace Dotnetable.Admin.Navigation;

/// <summary>One entry in the header search palette: a page the admin can jump to.</summary>
/// <param name="TitleKey">Localization key (matches the key used for the same link in NavMenu.razor).</param>
/// <param name="TitleDefault">Fallback title shown until translated.</param>
/// <param name="GroupKey">Localization key for the section label shown next to the result.</param>
/// <param name="GroupDefault">Fallback section label.</param>
/// <param name="RoleKey">Role key required to see this page, or null if every authenticated member can.</param>
/// <param name="SuperAdminOnly">True if only master-website members can reach this page.</param>
public sealed record AdminSearchPage(
    string TitleKey,
    string TitleDefault,
    string Href,
    string Icon,
    string GroupKey,
    string GroupDefault,
    string? RoleKey,
    bool SuperAdminOnly = false);

/// <summary>
/// Static index of every admin page for the header search palette. Keep this in sync with
/// NavMenu.razor: whenever a page/link is added there, add a matching entry here so it becomes
/// searchable (and remove it here if the page is removed there).
/// </summary>
public static class AdminSearchCatalog
{
    public static readonly IReadOnlyList<AdminSearchPage> Pages = new[]
    {
        new AdminSearchPage("dashboard", "Dashboard", "/", Icons.Material.Filled.Dashboard, "nav.navigation", "Navigation", null),

        new AdminSearchPage("members", "Members", "/members", Icons.Material.Filled.PersonOutline, "nav.user_management", "User Management", RoleKeys.MembersView),
        new AdminSearchPage("clients", "Customers", "/clients", Icons.Material.Filled.People, "nav.user_management", "User Management", RoleKeys.ClientsView),
        new AdminSearchPage("access_levels", "Access Levels", "/policies", Icons.Material.Filled.Shield, "nav.user_management", "User Management", RoleKeys.PoliciesView),
        new AdminSearchPage("roles", "Roles", "/roles", Icons.Material.Filled.VpnKey, "nav.user_management", "User Management", null, SuperAdminOnly: true),
        new AdminSearchPage("login_logs", "Login Logs", "/login-logs", Icons.Material.Filled.History, "nav.user_management", "User Management", RoleKeys.LoginLogsView),

        new AdminSearchPage("websites", "Websites", "/websites", Icons.Material.Filled.Language, "website", "Website", null, SuperAdminOnly: true),
        new AdminSearchPage("ip_whitelist", "IP Whitelist", "/website/ips", Icons.Material.Filled.Security, "website", "Website", RoleKeys.WebsiteEdit),
        new AdminSearchPage("scripts", "Scripts", "/website/scripts", Icons.Material.Filled.Code, "website", "Website", RoleKeys.WebsiteEdit),
        new AdminSearchPage("seo_settings", "SEO Settings", "/website/seo", Icons.Material.Filled.Search, "website", "Website", RoleKeys.WebsiteEdit),
        new AdminSearchPage("social_links", "Social Links", "/website/social", Icons.Material.Filled.Share, "website", "Website", RoleKeys.WebsiteEdit),
        new AdminSearchPage("website_api_key", "Website API Key", "/website/api-key", Icons.Material.Filled.VpnKey, "website", "Website", RoleKeys.WebsiteEdit),

        new AdminSearchPage("posts", "Posts", "/content/posts", Icons.Material.Filled.Article, "content", "Content", RoleKeys.PostsView),
        new AdminSearchPage("pages", "Pages", "/content/pages", Icons.Material.Filled.Description, "content", "Content", RoleKeys.PagesView),
        new AdminSearchPage("moderation.reviews", "Reviews", "/moderation/reviews", Icons.Material.Filled.Star, "nav.moderation", "Reviews & Q&A", RoleKeys.ModerationView),
        new AdminSearchPage("moderation.questions", "Questions", "/moderation/questions", Icons.Material.Filled.QuestionAnswer, "nav.moderation", "Reviews & Q&A", RoleKeys.ModerationView),
        new AdminSearchPage("menus", "Menus", "/menus", Icons.Material.Filled.Menu, "content", "Content", RoleKeys.MenusView),
        new AdminSearchPage("slideshows", "Slideshows", "/slideshows", Icons.Material.Filled.ViewCarousel, "content", "Content", RoleKeys.SlideshowsView),
        new AdminSearchPage("forms", "Forms & Surveys", "/forms", Icons.Material.Filled.DynamicForm, "content", "Content", RoleKeys.FormsView),
        new AdminSearchPage("themes", "Themes", "/themes", Icons.Material.Filled.Palette, "content", "Content", RoleKeys.ThemesView),
        new AdminSearchPage("categories", "Categories", "/content/categories", Icons.Material.Filled.Category, "content", "Content", RoleKeys.TaxonomyView),
        new AdminSearchPage("tags", "Tags", "/content/tags", Icons.Material.Filled.Label, "content", "Content", RoleKeys.TaxonomyView),
        new AdminSearchPage("post_types", "Post Types", "/content/post-types", Icons.Material.Filled.Dashboard, "content", "Content", RoleKeys.TaxonomyView),
        new AdminSearchPage("redirects", "Redirects", "/content/redirects", Icons.Material.Filled.CallSplit, "content", "Content", RoleKeys.RedirectsView),

        new AdminSearchPage("products", "Products", "/catalog/products", Icons.Material.Filled.ShoppingBag, "nav.catalog", "Catalog", RoleKeys.ProductsView),
        new AdminSearchPage("categories", "Categories", "/catalog/categories", Icons.Material.Filled.Category, "nav.catalog", "Catalog", RoleKeys.CatalogTaxonomyView),
        new AdminSearchPage("attributes", "Attributes", "/catalog/attributes", Icons.Material.Filled.Tune, "nav.catalog", "Catalog", RoleKeys.CatalogTaxonomyView),
        new AdminSearchPage("brands", "Brands", "/catalog/brands", Icons.Material.Filled.BrandingWatermark, "nav.catalog", "Catalog", RoleKeys.CatalogTaxonomyView),
        new AdminSearchPage("vendors", "Vendors", "/catalog/vendors", Icons.Material.Filled.Store, "nav.catalog", "Catalog", RoleKeys.VendorsView),

        new AdminSearchPage("inventory.stock", "Stock", "/inventory", Icons.Material.Filled.Numbers, "nav.inventory", "Inventory", RoleKeys.InventoryView),
        new AdminSearchPage("inventory.movements", "Movements", "/inventory/movements", Icons.Material.Filled.History, "nav.inventory", "Inventory", RoleKeys.InventoryView),
        new AdminSearchPage("suppliers", "Suppliers", "/inventory/suppliers", Icons.Material.Filled.LocalShipping, "nav.inventory", "Inventory", RoleKeys.SuppliersView),

        new AdminSearchPage("orders", "Orders", "/orders", Icons.Material.Filled.ReceiptLong, "orders", "Orders", RoleKeys.OrdersView),

        new AdminSearchPage("payments", "Payments", "/payments", Icons.Material.Filled.CreditCard, "nav.finance", "Finance", RoleKeys.PaymentsView),
        new AdminSearchPage("payments.refunds", "Bank Refunds", "/payments/refunds", Icons.Material.Filled.AssignmentReturn, "nav.finance", "Finance", RoleKeys.PaymentsRefund),
        new AdminSearchPage("wallets.withdrawals", "Withdrawals", "/wallets/withdrawals", Icons.Material.Filled.AccountBalanceWallet, "nav.finance", "Finance", RoleKeys.WalletsView),
        new AdminSearchPage("bank_accounts", "Bank Accounts", "/finance/bank-accounts", Icons.Material.Filled.AccountBalance, "nav.finance", "Finance", RoleKeys.BankAccountsView),
        new AdminSearchPage("currency_rates", "Exchange Rates", "/finance/currency-rates", Icons.Material.Filled.CurrencyExchange, "nav.finance", "Finance", RoleKeys.CurrencyView),
        new AdminSearchPage("banks", "Banks", "/finance/banks", Icons.Material.Filled.AccountBalance, "nav.finance", "Finance", null, SuperAdminOnly: true),
        new AdminSearchPage("currencies", "Currencies", "/finance/currencies", Icons.Material.Filled.CurrencyExchange, "nav.finance", "Finance", null, SuperAdminOnly: true),

        new AdminSearchPage("coupons", "Coupons", "/sales/coupons", Icons.Material.Filled.Discount, "nav.promotions", "Promotions & Shipping", RoleKeys.CouponsView),
        new AdminSearchPage("shipping", "Shipping", "/sales/shipping", Icons.Material.Filled.LocalShipping, "nav.promotions", "Promotions & Shipping", RoleKeys.ShippingView),
        new AdminSearchPage("tax", "Tax", "/sales/tax", Icons.Material.Filled.Percent, "nav.promotions", "Promotions & Shipping", RoleKeys.TaxView),

        new AdminSearchPage("contact_messages", "Contact Messages", "/messages/contacts", Icons.Material.Filled.ContactMail, "messages", "Messages", RoleKeys.MessagesView),
        new AdminSearchPage("email_settings", "Email Settings", "/messages/email", Icons.Material.Filled.Email, "messages", "Messages", RoleKeys.WebsiteEdit),

        new AdminSearchPage("translations", "Translations", "/translations", Icons.Material.Filled.Translate, "translations", "Translations", RoleKeys.LocalizationEdit),

        new AdminSearchPage("media_library", "Media Library", "/media", Icons.Material.Filled.PhotoLibrary, "media", "Media", RoleKeys.MediaView),
        new AdminSearchPage("storage", "Storage", "/media/storage", Icons.Material.Filled.Cloud, "media", "Media", RoleKeys.MediaUpload),
        new AdminSearchPage("watermark", "Watermark", "/media/watermark", Icons.Material.Filled.BrandingWatermark, "media", "Media", RoleKeys.MediaUpload),

        new AdminSearchPage("countries", "Countries", "/initial-data/countries", Icons.Material.Filled.Flag, "nav.administration", "Administration", null, SuperAdminOnly: true),
        new AdminSearchPage("states", "States / Provinces", "/initial-data/states", Icons.Material.Filled.Map, "nav.administration", "Administration", null, SuperAdminOnly: true),
        new AdminSearchPage("cities", "Cities", "/initial-data/cities", Icons.Material.Filled.LocationCity, "nav.administration", "Administration", null, SuperAdminOnly: true),

        new AdminSearchPage("db_updates", "DB Updates", "/system/updates", Icons.Material.Filled.SystemUpdateAlt, "nav.system", "System", null, SuperAdminOnly: true),
        new AdminSearchPage("settings", "Settings", "/settings", Icons.Material.Filled.Settings, "nav.system", "System", null, SuperAdminOnly: true),
    };
}
