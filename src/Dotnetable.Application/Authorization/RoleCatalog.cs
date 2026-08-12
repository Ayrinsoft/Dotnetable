namespace Dotnetable.Application.Authorization;

/// <summary>Distinguishes admin-panel permissions from website-customer (client) permissions.</summary>
public enum RoleCategory : byte
{
    /// <summary>Permission used inside the admin panel.</summary>
    Admin = 0,

    /// <summary>Permission for website customers (purchase, review, profile, …) — never shown in admin.</summary>
    Client = 1,
}

/// <summary>A single seedable permission.</summary>
public sealed record RoleDefinition(string Key, string Description, RoleCategory Category)
{
    /// <summary>The "area" segment of the key (text before the first dot) used to group keys into a tree.</summary>
    public string Group => Key.Split('.', 2)[0];
}

/// <summary>
/// The full set of permissions the system ships with. Seeded on first run and used to register
/// one authorization policy per admin key. Adding a new permission here (plus a migration-free seed
/// top-up) is all that is needed to introduce a new page/action guard.
/// </summary>
public static class RoleCatalog
{
    public static readonly IReadOnlyList<RoleDefinition> All =
    [
        // Members
        new(RoleKeys.MembersView, "View members", RoleCategory.Admin),
        new(RoleKeys.MembersInsert, "Create members", RoleCategory.Admin),
        new(RoleKeys.MembersEdit, "Edit members", RoleCategory.Admin),
        new(RoleKeys.MembersDelete, "Delete members", RoleCategory.Admin),

        // Website customers (client management)
        new(RoleKeys.ClientsView, "View website customers", RoleCategory.Admin),
        new(RoleKeys.ClientsInsert, "Create website customers", RoleCategory.Admin),
        new(RoleKeys.ClientsEdit, "Edit website customers", RoleCategory.Admin),
        new(RoleKeys.ClientsDelete, "Delete website customers", RoleCategory.Admin),

        // Access levels (policies)
        new(RoleKeys.PoliciesView, "View access levels", RoleCategory.Admin),
        new(RoleKeys.PoliciesInsert, "Create access levels", RoleCategory.Admin),
        new(RoleKeys.PoliciesEdit, "Edit access levels", RoleCategory.Admin),
        new(RoleKeys.PoliciesDelete, "Delete access levels", RoleCategory.Admin),

        // Website settings
        new(RoleKeys.WebsiteView, "View website settings", RoleCategory.Admin),
        new(RoleKeys.WebsiteEdit, "Edit website settings", RoleCategory.Admin),

        // Messages
        new(RoleKeys.MessagesView, "View contact messages", RoleCategory.Admin),
        new(RoleKeys.MessagesDelete, "Delete contact messages", RoleCategory.Admin),

        // Localization
        new(RoleKeys.LocalizationView, "View translations", RoleCategory.Admin),
        new(RoleKeys.LocalizationEdit, "Edit translations", RoleCategory.Admin),

        // Navigation menus
        new(RoleKeys.MenusView, "View navigation menus", RoleCategory.Admin),
        new(RoleKeys.MenusInsert, "Create navigation menus", RoleCategory.Admin),
        new(RoleKeys.MenusEdit, "Edit navigation menus", RoleCategory.Admin),
        new(RoleKeys.MenusDelete, "Delete navigation menus", RoleCategory.Admin),

        // Media
        new(RoleKeys.MediaView, "View media library", RoleCategory.Admin),
        new(RoleKeys.MediaUpload, "Upload media & manage storage", RoleCategory.Admin),
        new(RoleKeys.MediaDelete, "Delete media", RoleCategory.Admin),

        // Slideshows
        new(RoleKeys.SlideshowsView, "View slideshows", RoleCategory.Admin),
        new(RoleKeys.SlideshowsInsert, "Create slideshows", RoleCategory.Admin),
        new(RoleKeys.SlideshowsEdit, "Edit slideshows", RoleCategory.Admin),
        new(RoleKeys.SlideshowsDelete, "Delete slideshows", RoleCategory.Admin),

        // Forms & surveys
        new(RoleKeys.FormsView, "View forms & surveys", RoleCategory.Admin),
        new(RoleKeys.FormsInsert, "Create forms & surveys", RoleCategory.Admin),
        new(RoleKeys.FormsEdit, "Edit forms & surveys", RoleCategory.Admin),
        new(RoleKeys.FormsDelete, "Delete forms & surveys", RoleCategory.Admin),
        new(RoleKeys.FormsReport, "View form reports & responses", RoleCategory.Admin),

        // Themes
        new(RoleKeys.ThemesView, "View website theme packages", RoleCategory.Admin),
        new(RoleKeys.ThemesEdit, "Install/activate/delete website themes", RoleCategory.Admin),

        // Content — posts
        new(RoleKeys.PostsView, "View posts", RoleCategory.Admin),
        new(RoleKeys.PostsInsert, "Create posts", RoleCategory.Admin),
        new(RoleKeys.PostsEdit, "Edit posts", RoleCategory.Admin),
        new(RoleKeys.PostsDelete, "Delete posts", RoleCategory.Admin),

        // Content — pages
        new(RoleKeys.PagesView, "View pages", RoleCategory.Admin),
        new(RoleKeys.PagesInsert, "Create pages", RoleCategory.Admin),
        new(RoleKeys.PagesEdit, "Edit pages", RoleCategory.Admin),
        new(RoleKeys.PagesDelete, "Delete pages", RoleCategory.Admin),

        // Content — taxonomy (categories, tags, post types)
        new(RoleKeys.TaxonomyView, "View categories, tags & post types", RoleCategory.Admin),
        new(RoleKeys.TaxonomyInsert, "Create categories, tags & post types", RoleCategory.Admin),
        new(RoleKeys.TaxonomyEdit, "Edit categories, tags & post types", RoleCategory.Admin),
        new(RoleKeys.TaxonomyDelete, "Delete categories, tags & post types", RoleCategory.Admin),

        // Website redirects
        new(RoleKeys.RedirectsView, "View website redirects", RoleCategory.Admin),
        new(RoleKeys.RedirectsInsert, "Create website redirects", RoleCategory.Admin),
        new(RoleKeys.RedirectsEdit, "Edit website redirects", RoleCategory.Admin),
        new(RoleKeys.RedirectsDelete, "Delete website redirects", RoleCategory.Admin),

        // Login logs
        new(RoleKeys.LoginLogsView, "View login logs", RoleCategory.Admin),

        // Catalog — products
        new(RoleKeys.ProductsView, "View products", RoleCategory.Admin),
        new(RoleKeys.ProductsInsert, "Create products", RoleCategory.Admin),
        new(RoleKeys.ProductsEdit, "Edit products", RoleCategory.Admin),
        new(RoleKeys.ProductsDelete, "Delete products", RoleCategory.Admin),

        // Catalog taxonomy — categories, attributes, brands
        new(RoleKeys.CatalogTaxonomyView, "View catalog categories, attributes & brands", RoleCategory.Admin),
        new(RoleKeys.CatalogTaxonomyInsert, "Create catalog categories, attributes & brands", RoleCategory.Admin),
        new(RoleKeys.CatalogTaxonomyEdit, "Edit catalog categories, attributes & brands", RoleCategory.Admin),
        new(RoleKeys.CatalogTaxonomyDelete, "Delete catalog categories, attributes & brands", RoleCategory.Admin),

        // Vendors
        new(RoleKeys.VendorsView, "View vendors", RoleCategory.Admin),
        new(RoleKeys.VendorsInsert, "Create vendors", RoleCategory.Admin),
        new(RoleKeys.VendorsEdit, "Edit vendors", RoleCategory.Admin),
        new(RoleKeys.VendorsDelete, "Delete vendors", RoleCategory.Admin),

        // Inventory
        new(RoleKeys.InventoryView, "View inventory / stock", RoleCategory.Admin),
        new(RoleKeys.InventoryEdit, "Adjust inventory / stock", RoleCategory.Admin),

        // Suppliers
        new(RoleKeys.SuppliersView, "View suppliers", RoleCategory.Admin),
        new(RoleKeys.SuppliersInsert, "Create suppliers", RoleCategory.Admin),
        new(RoleKeys.SuppliersEdit, "Edit suppliers", RoleCategory.Admin),
        new(RoleKeys.SuppliersDelete, "Delete suppliers", RoleCategory.Admin),

        // Customer wallets
        new(RoleKeys.WalletsView, "View customer wallets", RoleCategory.Admin),
        new(RoleKeys.WalletsApprove, "Approve / reject wallet withdrawals", RoleCategory.Admin),
        new(RoleKeys.WalletsAdjust, "Manually adjust wallet balances", RoleCategory.Admin),

        // Coupons
        new(RoleKeys.CouponsView, "View coupons", RoleCategory.Admin),
        new(RoleKeys.CouponsInsert, "Create coupons", RoleCategory.Admin),
        new(RoleKeys.CouponsEdit, "Edit coupons", RoleCategory.Admin),
        new(RoleKeys.CouponsDelete, "Delete coupons", RoleCategory.Admin),

        // Shipping
        new(RoleKeys.ShippingView, "View shipping methods & rates", RoleCategory.Admin),
        new(RoleKeys.ShippingInsert, "Create shipping methods & rates", RoleCategory.Admin),
        new(RoleKeys.ShippingEdit, "Edit shipping methods & rates", RoleCategory.Admin),
        new(RoleKeys.ShippingDelete, "Delete shipping methods & rates", RoleCategory.Admin),

        // Tax
        new(RoleKeys.TaxView, "View tax rates", RoleCategory.Admin),
        new(RoleKeys.TaxInsert, "Create tax rates", RoleCategory.Admin),
        new(RoleKeys.TaxEdit, "Edit tax rates", RoleCategory.Admin),
        new(RoleKeys.TaxDelete, "Delete tax rates", RoleCategory.Admin),

        // Settlements
        new(RoleKeys.SettlementsView, "View settlements", RoleCategory.Admin),
        new(RoleKeys.SettlementsEdit, "Approve / pay / cancel settlements", RoleCategory.Admin),

        new(RoleKeys.AccountingView, "View chart of accounts & journals", RoleCategory.Admin),
        new(RoleKeys.AccountingEdit, "Edit chart of accounts & draft journals", RoleCategory.Admin),
        new(RoleKeys.AccountingPost, "Post / reverse journals", RoleCategory.Admin),
        new(RoleKeys.AccountingReport, "View trial balance & P&L", RoleCategory.Admin),

        // Currency
        new(RoleKeys.CurrencyView, "View currencies & exchange rates", RoleCategory.Admin),
        new(RoleKeys.CurrencyEdit, "Edit currencies & exchange rates", RoleCategory.Admin),

        // Orders
        new(RoleKeys.OrdersView, "View orders", RoleCategory.Admin),
        new(RoleKeys.OrdersEdit, "Create/edit orders, manual social sales, status transitions", RoleCategory.Admin),

        // Payments
        new(RoleKeys.PaymentsView, "View payments", RoleCategory.Admin),
        new(RoleKeys.PaymentsVerify, "Verify bank receipts and record offline/COD payments received", RoleCategory.Admin),
        new(RoleKeys.PaymentsRefund, "Process refunds (wallet, bank, or cash)", RoleCategory.Admin),

        // Website's own bank accounts
        new(RoleKeys.BankAccountsView, "View website bank accounts", RoleCategory.Admin),
        new(RoleKeys.BankAccountsInsert, "Create website bank accounts", RoleCategory.Admin),
        new(RoleKeys.BankAccountsEdit, "Edit website bank accounts", RoleCategory.Admin),
        new(RoleKeys.BankAccountsDelete, "Delete website bank accounts", RoleCategory.Admin),

        // Product review / Q&A moderation
        new(RoleKeys.ModerationView, "View product reviews & questions", RoleCategory.Admin),
        new(RoleKeys.ModerationReview, "Approve / reject reviews & questions", RoleCategory.Admin),

        // Support desk / Customer 360
        new(RoleKeys.SupportView, "View support desk, customer 360, tickets", RoleCategory.Admin),
        new(RoleKeys.SupportEdit, "Create/edit support tickets and log interactions", RoleCategory.Admin),

        // Client (website customers)
        new(RoleKeys.ClientAccess, "Sign in and general site access", RoleCategory.Client),
        new(RoleKeys.ClientPurchase, "Place orders / purchase", RoleCategory.Client),
        new(RoleKeys.ClientReview, "Post product reviews / comments", RoleCategory.Client),
        new(RoleKeys.ClientProfile, "Manage own profile", RoleCategory.Client),
    ];

    /// <summary>Admin permission keys — one authorization policy is registered per key.</summary>
    public static readonly IReadOnlyList<string> AdminKeys =
        All.Where(r => r.Category == RoleCategory.Admin).Select(r => r.Key).ToList();

    /// <summary>Client permission keys (website customers).</summary>
    public static readonly IReadOnlyList<string> ClientKeys =
        All.Where(r => r.Category == RoleCategory.Client).Select(r => r.Key).ToList();
}
