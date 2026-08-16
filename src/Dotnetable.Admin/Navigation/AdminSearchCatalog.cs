using Dotnetable.Admin.Auth;
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
/// <param name="NavArea">Optional surface area for Basic/General UiMode filtering (null = always allowed when roles match).</param>
/// <param name="VendorMemberAllowed">When false, marketplace seller accounts never see this page in search/nav surface.</param>
public sealed record AdminSearchPage(
    string TitleKey,
    string TitleDefault,
    string Href,
    string Icon,
    string GroupKey,
    string GroupDefault,
    string? RoleKey,
    bool SuperAdminOnly = false,
    AdminNavArea? NavArea = null,
    bool VendorMemberAllowed = true);

/// <summary>
/// Static index of every admin page for the header search palette. Keep this in sync with
/// NavMenu.razor: whenever a page/link is added there, add a matching entry here so it becomes
/// searchable (and remove it here if the page is removed there).
/// Also update the matching guide page in <c>src/Dotnetable.Docs/wwwroot/js/catalog-admin.js</c>
/// (and API cross-links in <c>catalog-api.js</c> when the same concept is exposed publicly).
/// </summary>
public static class AdminSearchCatalog
{
    public static readonly IReadOnlyList<AdminSearchPage> Pages = new[]
    {
        new AdminSearchPage("dashboard", "Dashboard", "/", Icons.Material.Filled.Dashboard, "nav.navigation", "Navigation", null),
        new AdminSearchPage("inbox", "Inbox", "/inbox", Icons.Material.Filled.Inbox, "nav.navigation", "Navigation", null),
        new AdminSearchPage("notifications", "Notifications", "/notifications", Icons.Material.Filled.Notifications, "nav.navigation", "Navigation", null),

        new AdminSearchPage("members", "Members", "/members", Icons.Material.Filled.PersonOutline, "nav.user_management", "User Management", RoleKeys.MembersView, NavArea: AdminNavArea.Users, VendorMemberAllowed: false),
        new AdminSearchPage("clients", "Customers", "/clients", Icons.Material.Filled.People, "nav.user_management", "User Management", RoleKeys.ClientsView, NavArea: AdminNavArea.Users, VendorMemberAllowed: false),
        new AdminSearchPage("access_levels", "Access Levels", "/policies", Icons.Material.Filled.Shield, "nav.user_management", "User Management", RoleKeys.PoliciesView, NavArea: AdminNavArea.Users, VendorMemberAllowed: false),
        new AdminSearchPage("roles", "Roles", "/roles", Icons.Material.Filled.VpnKey, "nav.user_management", "User Management", null, SuperAdminOnly: true, VendorMemberAllowed: false),
        new AdminSearchPage("login_logs", "Login Logs", "/login-logs", Icons.Material.Filled.History, "nav.user_management", "User Management", RoleKeys.LoginLogsView, NavArea: AdminNavArea.Users, VendorMemberAllowed: false),

        new AdminSearchPage("websites", "Websites", "/websites", Icons.Material.Filled.Public, "website", "Website", null, SuperAdminOnly: true, VendorMemberAllowed: false),
        new AdminSearchPage("ip_whitelist", "IP Whitelist", "/website/ips", Icons.Material.Filled.Security, "website", "Website", RoleKeys.WebsiteEdit, NavArea: AdminNavArea.Website, VendorMemberAllowed: false),
        new AdminSearchPage("scripts", "Scripts", "/website/scripts", Icons.Material.Filled.Code, "website", "Website", RoleKeys.WebsiteEdit, NavArea: AdminNavArea.Website, VendorMemberAllowed: false),
        new AdminSearchPage("seo_settings", "SEO Settings", "/website/seo", Icons.Material.Filled.Search, "website", "Website", RoleKeys.WebsiteEdit, NavArea: AdminNavArea.Website, VendorMemberAllowed: false),
        new AdminSearchPage("social_links", "Social Links", "/website/social", Icons.Material.Filled.Share, "website", "Website", RoleKeys.WebsiteEdit, NavArea: AdminNavArea.Website, VendorMemberAllowed: false),
        new AdminSearchPage("website_api_key", "Website API Key", "/website/api-key", Icons.Material.Filled.VpnKey, "website", "Website", RoleKeys.WebsiteEdit, NavArea: AdminNavArea.Website, VendorMemberAllowed: false),
        new AdminSearchPage("website_languages", "Languages", "/website/languages", Icons.Material.Filled.Language, "website", "Website", RoleKeys.LocalizationEdit, NavArea: AdminNavArea.Website, VendorMemberAllowed: false),
        new AdminSearchPage("website_translations", "Translations", "/website/translations", Icons.Material.Filled.Translate, "website", "Website", RoleKeys.LocalizationEdit, NavArea: AdminNavArea.Website, VendorMemberAllowed: false),

        new AdminSearchPage("posts", "Posts", "/content/posts", Icons.Material.Filled.Article, "content", "Content", RoleKeys.PostsView, NavArea: AdminNavArea.ContentBlog, VendorMemberAllowed: false),
        new AdminSearchPage("pages", "Pages", "/content/pages", Icons.Material.Filled.Description, "content", "Content", RoleKeys.PagesView, NavArea: AdminNavArea.ContentCore, VendorMemberAllowed: false),
        new AdminSearchPage("moderation.reviews", "Reviews", "/moderation/reviews", Icons.Material.Filled.Star, "nav.moderation", "Reviews & Q&A", RoleKeys.ModerationView, NavArea: AdminNavArea.ContentExtra, VendorMemberAllowed: false),
        new AdminSearchPage("moderation.questions", "Questions", "/moderation/questions", Icons.Material.Filled.QuestionAnswer, "nav.moderation", "Reviews & Q&A", RoleKeys.ModerationView, NavArea: AdminNavArea.ContentExtra, VendorMemberAllowed: false),
        new AdminSearchPage("menus", "Menus", "/menus", Icons.Material.Filled.Menu, "content", "Content", RoleKeys.MenusView, NavArea: AdminNavArea.ContentCore, VendorMemberAllowed: false),
        new AdminSearchPage("slideshows", "Slideshows", "/slideshows", Icons.Material.Filled.ViewCarousel, "content", "Content", RoleKeys.SlideshowsView, NavArea: AdminNavArea.ContentCore, VendorMemberAllowed: false),
        new AdminSearchPage("forms", "Forms & Surveys", "/forms", Icons.Material.Filled.DynamicForm, "content", "Content", RoleKeys.FormsView, NavArea: AdminNavArea.ContentExtra, VendorMemberAllowed: false),
        new AdminSearchPage("themes", "Themes", "/themes", Icons.Material.Filled.Palette, "content", "Content", RoleKeys.ThemesView, NavArea: AdminNavArea.ContentCore, VendorMemberAllowed: false),
        new AdminSearchPage("categories", "Categories", "/content/categories", Icons.Material.Filled.Category, "content", "Content", RoleKeys.TaxonomyView, NavArea: AdminNavArea.ContentBlog, VendorMemberAllowed: false),
        new AdminSearchPage("tags", "Tags", "/content/tags", Icons.Material.Filled.Label, "content", "Content", RoleKeys.TaxonomyView, NavArea: AdminNavArea.ContentBlog, VendorMemberAllowed: false),
        new AdminSearchPage("post_types", "Post Types", "/content/post-types", Icons.Material.Filled.Dashboard, "content", "Content", RoleKeys.TaxonomyView, NavArea: AdminNavArea.ContentBlog, VendorMemberAllowed: false),
        new AdminSearchPage("redirects", "Redirects", "/content/redirects", Icons.Material.Filled.CallSplit, "content", "Content", RoleKeys.RedirectsView, NavArea: AdminNavArea.ContentExtra, VendorMemberAllowed: false),

        new AdminSearchPage("products", "Products", "/catalog/products", Icons.Material.Filled.ShoppingBag, "nav.catalog", "Catalog", RoleKeys.ProductsView, NavArea: AdminNavArea.Catalog),
        new AdminSearchPage("categories", "Categories", "/catalog/categories", Icons.Material.Filled.Category, "nav.catalog", "Catalog", RoleKeys.CatalogTaxonomyView, NavArea: AdminNavArea.Catalog, VendorMemberAllowed: false),
        new AdminSearchPage("attributes", "Attributes", "/catalog/attributes", Icons.Material.Filled.Tune, "nav.catalog", "Catalog", RoleKeys.CatalogTaxonomyView, NavArea: AdminNavArea.Catalog, VendorMemberAllowed: false),
        new AdminSearchPage("brands", "Brands", "/catalog/brands", Icons.Material.Filled.BrandingWatermark, "nav.catalog", "Catalog", RoleKeys.CatalogTaxonomyView, NavArea: AdminNavArea.Catalog, VendorMemberAllowed: false),
        new AdminSearchPage("warranties", "Warranties", "/catalog/warranties", Icons.Material.Filled.VerifiedUser, "nav.catalog", "Catalog", RoleKeys.CatalogTaxonomyView, NavArea: AdminNavArea.Catalog, VendorMemberAllowed: false),
        new AdminSearchPage("vendors", "Vendors", "/catalog/vendors", Icons.Material.Filled.Store, "nav.catalog", "Catalog", RoleKeys.VendorsView, NavArea: AdminNavArea.Catalog, VendorMemberAllowed: false),

        new AdminSearchPage("inventory.stock", "Stock", "/inventory", Icons.Material.Filled.Numbers, "nav.inventory", "Inventory", RoleKeys.InventoryView, NavArea: AdminNavArea.Inventory, VendorMemberAllowed: false),
        new AdminSearchPage("inventory.movements", "Movements", "/inventory/movements", Icons.Material.Filled.History, "nav.inventory", "Inventory", RoleKeys.InventoryView, NavArea: AdminNavArea.Inventory, VendorMemberAllowed: false),
        new AdminSearchPage("warehouse.list", "Warehouses", "/inventory/warehouses", Icons.Material.Filled.HomeWork, "nav.inventory", "Inventory", RoleKeys.WarehouseView, NavArea: AdminNavArea.Inventory, VendorMemberAllowed: false),
        new AdminSearchPage("warehouse.tasks", "Warehouse my tasks", "/inventory/my-tasks", Icons.Material.Filled.Checklist, "nav.inventory", "Inventory", RoleKeys.WarehouseView, NavArea: AdminNavArea.Inventory, VendorMemberAllowed: false),
        new AdminSearchPage("warehouse.docs", "Stock documents", "/inventory/stock-documents", Icons.Material.Filled.Assignment, "nav.inventory", "Inventory", RoleKeys.WarehouseView, NavArea: AdminNavArea.Inventory, VendorMemberAllowed: false),
        new AdminSearchPage("returns.title", "Customer returns", "/inventory/returns", Icons.Material.Filled.AssignmentReturn, "nav.inventory", "Inventory", RoleKeys.WarehouseView, NavArea: AdminNavArea.Inventory, VendorMemberAllowed: false),
        new AdminSearchPage("suppliers", "Suppliers", "/inventory/suppliers", Icons.Material.Filled.LocalShipping, "nav.inventory", "Inventory", RoleKeys.SuppliersView, NavArea: AdminNavArea.Inventory, VendorMemberAllowed: false),

        new AdminSearchPage("orders", "Orders", "/orders", Icons.Material.Filled.ReceiptLong, "orders", "Orders", RoleKeys.OrdersView, NavArea: AdminNavArea.Orders),
        new AdminSearchPage("orders.create", "Create order", "/orders/new", Icons.Material.Filled.AddShoppingCart, "orders.create", "Create order", RoleKeys.OrdersEdit, NavArea: AdminNavArea.Orders),

        new AdminSearchPage("support.desk", "Support Desk", "/support", Icons.Material.Filled.SupportAgent, "nav.support", "Support", RoleKeys.SupportView, NavArea: AdminNavArea.Orders, VendorMemberAllowed: false),
        new AdminSearchPage("support.tickets", "Support Tickets", "/support/tickets", Icons.Material.Filled.ConfirmationNumber, "nav.support", "Support", RoleKeys.SupportView, NavArea: AdminNavArea.Orders, VendorMemberAllowed: false),

        new AdminSearchPage("hr.payroll", "Payroll", "/hr/payroll", Icons.Material.Filled.Payments, "nav.hr", "HR", RoleKeys.PayrollView, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("hr.payroll_reports", "Payroll reports", "/hr/payroll/reports", Icons.Material.Filled.Assessment, "nav.hr", "HR", RoleKeys.PayrollView, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("hr.rate_brackets", "Payroll rate brackets", "/hr/payroll/rate-brackets", Icons.Material.Filled.StackedLineChart, "nav.hr", "HR", RoleKeys.PayrollRun, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("tax.periods", "Tax periods", "/sales/tax/periods", Icons.Material.Filled.DateRange, "nav.promotions", "Tax", RoleKeys.TaxView, NavArea: AdminNavArea.Promotions, VendorMemberAllowed: false),

        new AdminSearchPage("payments", "Payments", "/payments", Icons.Material.Filled.CreditCard, "nav.finance", "Finance", RoleKeys.PaymentsView, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("payments.refunds", "Bank Refunds", "/payments/refunds", Icons.Material.Filled.AssignmentReturn, "nav.finance", "Finance", RoleKeys.PaymentsRefund, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("wallets", "Customer wallets", "/wallets", Icons.Material.Filled.AccountBalanceWallet, "nav.finance", "Finance", RoleKeys.WalletsView, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("wallets.withdrawals", "Withdrawals", "/wallets/withdrawals", Icons.Material.Filled.Outbound, "nav.finance", "Finance", RoleKeys.WalletsView, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("bank_accounts", "Bank Accounts", "/finance/bank-accounts", Icons.Material.Filled.AccountBalance, "nav.finance", "Finance", RoleKeys.BankAccountsView, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("currency_rates", "Exchange Rates", "/finance/currency-rates", Icons.Material.Filled.CurrencyExchange, "nav.finance", "Finance", RoleKeys.CurrencyView, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("banks", "Banks", "/finance/banks", Icons.Material.Filled.AccountBalance, "nav.finance", "Finance", null, SuperAdminOnly: true, VendorMemberAllowed: false),
        new AdminSearchPage("currencies", "Currencies", "/finance/currencies", Icons.Material.Filled.CurrencyExchange, "nav.finance", "Finance", null, SuperAdminOnly: true, VendorMemberAllowed: false),

        new AdminSearchPage("coupons", "Coupons", "/sales/coupons", Icons.Material.Filled.Discount, "nav.promotions", "Promotions & Shipping", RoleKeys.CouponsView, NavArea: AdminNavArea.Promotions, VendorMemberAllowed: false),
        new AdminSearchPage("shipping", "Shipping", "/sales/shipping", Icons.Material.Filled.LocalShipping, "nav.promotions", "Promotions & Shipping", RoleKeys.ShippingView, NavArea: AdminNavArea.Promotions, VendorMemberAllowed: false),
        new AdminSearchPage("tax", "Tax", "/sales/tax", Icons.Material.Filled.Percent, "nav.promotions", "Promotions & Shipping", RoleKeys.TaxView, NavArea: AdminNavArea.Promotions, VendorMemberAllowed: false),
        new AdminSearchPage("vat_report", "VAT report", "/sales/tax/report", Icons.Material.Filled.Assessment, "nav.promotions", "Promotions & Shipping", RoleKeys.TaxView, NavArea: AdminNavArea.Promotions, VendorMemberAllowed: false),
        new AdminSearchPage("ledger", "Financial ledger", "/finance/ledger", Icons.Material.Filled.AccountBalanceWallet, "nav.finance", "Finance", RoleKeys.SettlementsView, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("settlements", "Settlements", "/finance/settlements", Icons.Material.Filled.Handshake, "nav.finance", "Finance", RoleKeys.SettlementsView, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("settlements.fx_report", "Settlement FX / cost report", "/finance/settlements/report", Icons.Material.Filled.Assessment, "nav.finance", "Finance", RoleKeys.SettlementsView, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("accounting.coa", "Chart of accounts", "/finance/coa", Icons.Material.Filled.AccountTree, "nav.finance", "Finance", RoleKeys.AccountingView, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("accounting.journals", "Journals", "/finance/journals", Icons.Material.Filled.MenuBook, "nav.finance", "Finance", RoleKeys.AccountingView, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("accounting.trial_balance", "Trial balance", "/finance/reports/trial-balance", Icons.Material.Filled.TableChart, "nav.finance", "Finance", RoleKeys.AccountingReport, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("accounting.pnl", "Profit & loss", "/finance/reports/pnl", Icons.Material.Filled.ShowChart, "nav.finance", "Finance", RoleKeys.AccountingReport, NavArea: AdminNavArea.Finance, VendorMemberAllowed: false),
        new AdminSearchPage("ledger.my_earnings", "My settlements", "/finance/my-earnings", Icons.Material.Filled.Payments, "nav.finance", "Finance", RoleKeys.OrdersView, NavArea: AdminNavArea.Orders, VendorMemberAllowed: true),

        new AdminSearchPage("contact_messages", "Contact Messages", "/messages/contacts", Icons.Material.Filled.ContactMail, "messages", "Messages", RoleKeys.MessagesView, NavArea: AdminNavArea.Messages),
        new AdminSearchPage("email_accounts", "Email Accounts", "/messages/email-accounts", Icons.Material.Filled.AlternateEmail, "messages", "Messages", RoleKeys.WebsiteEdit, NavArea: AdminNavArea.Messages, VendorMemberAllowed: false),
        new AdminSearchPage("email_templates", "Email Templates", "/messages/email-templates", Icons.Material.Filled.Description, "messages", "Messages", RoleKeys.WebsiteEdit, NavArea: AdminNavArea.Messages, VendorMemberAllowed: false),

        new AdminSearchPage("media_library", "Media Library", "/media", Icons.Material.Filled.PhotoLibrary, "media", "Media", RoleKeys.MediaView, NavArea: AdminNavArea.Media),
        new AdminSearchPage("media_folders", "Media Folders", "/media/folders", Icons.Material.Filled.Folder, "media", "Media", RoleKeys.MediaView, NavArea: AdminNavArea.Media, VendorMemberAllowed: false),
        new AdminSearchPage("media_tags", "Media Tags", "/media/tags", Icons.Material.Filled.LocalOffer, "media", "Media", RoleKeys.MediaView, NavArea: AdminNavArea.Media, VendorMemberAllowed: false),
        new AdminSearchPage("storage", "Storage", "/media/storage", Icons.Material.Filled.Cloud, "media", "Media", RoleKeys.MediaUpload, NavArea: AdminNavArea.Media, VendorMemberAllowed: false),
        new AdminSearchPage("watermark", "Watermark", "/media/watermark", Icons.Material.Filled.BrandingWatermark, "media", "Media", RoleKeys.MediaUpload, NavArea: AdminNavArea.Media, VendorMemberAllowed: false),

        new AdminSearchPage("countries", "Countries", "/initial-data/countries", Icons.Material.Filled.Flag, "nav.administration", "Administration", null, SuperAdminOnly: true, VendorMemberAllowed: false),
        new AdminSearchPage("states", "States / Provinces", "/initial-data/states", Icons.Material.Filled.Map, "nav.administration", "Administration", null, SuperAdminOnly: true, VendorMemberAllowed: false),
        new AdminSearchPage("cities", "Cities", "/initial-data/cities", Icons.Material.Filled.LocationCity, "nav.administration", "Administration", null, SuperAdminOnly: true, VendorMemberAllowed: false),
        new AdminSearchPage("languages.catalog", "Language Catalog", "/languages", Icons.Material.Filled.Language, "initial_data", "Initial Data", null, SuperAdminOnly: true, VendorMemberAllowed: false),
        new AdminSearchPage("translations.admin", "Admin Translations", "/translations", Icons.Material.Filled.Translate, "initial_data", "Initial Data", null, SuperAdminOnly: true, VendorMemberAllowed: false),

        new AdminSearchPage("db_updates", "DB Updates", "/system/updates", Icons.Material.Filled.SystemUpdateAlt, "nav.system", "System", null, SuperAdminOnly: true, VendorMemberAllowed: false),
        new AdminSearchPage("settings", "Settings", "/settings", Icons.Material.Filled.Settings, "nav.system", "System", null, SuperAdminOnly: true, VendorMemberAllowed: false),
    };
}
