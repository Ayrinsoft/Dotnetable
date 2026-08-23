namespace Dotnetable.Application.Authorization;

/// <summary>
/// Canonical permission keys stored in the Role table (Role.RoleKey).
/// Keys follow an "area.action" convention so the admin UI can render them as a tree
/// (the part before the dot is the group, the part after is the action).
/// Authorization is role-based: every page/action requires one specific key, and a member
/// is allowed only when the key is present in the roles granted through their policy
/// (master-website members bypass the check entirely).
/// </summary>
public static class RoleKeys
{
    // ── Members (admin) ─────────────────────────────────────────────
    public const string MembersView = "members.view";
    public const string MembersInsert = "members.insert";
    public const string MembersEdit = "members.edit";
    public const string MembersDelete = "members.delete";

    // ── Website customers / clients (admin management) ──────────────
    public const string ClientsView = "clients.view";
    public const string ClientsInsert = "clients.insert";
    public const string ClientsEdit = "clients.edit";
    public const string ClientsDelete = "clients.delete";

    // ── Access levels / policies (admin) ────────────────────────────
    public const string PoliciesView = "policies.view";
    public const string PoliciesInsert = "policies.insert";
    public const string PoliciesEdit = "policies.edit";
    public const string PoliciesDelete = "policies.delete";

    // ── Website settings: IP, scripts, SEO, social, email (admin) ───
    public const string WebsiteView = "website.view";
    public const string WebsiteEdit = "website.edit";

    // ── Contact messages (admin) ────────────────────────────────────
    public const string MessagesView = "messages.view";
    public const string MessagesDelete = "messages.delete";

    // ── Localization / translations (admin) ─────────────────────────
    public const string LocalizationView = "localization.view";
    public const string LocalizationEdit = "localization.edit";

    // ── Navigation menus (admin) ────────────────────────────────────
    public const string MenusView = "menus.view";
    public const string MenusInsert = "menus.insert";
    public const string MenusEdit = "menus.edit";
    public const string MenusDelete = "menus.delete";

    // ── Media library (admin) ───────────────────────────────────────
    public const string MediaView = "media.view";
    public const string MediaUpload = "media.upload";
    public const string MediaDelete = "media.delete";

    // ── Slideshows (admin) ───────────────────────────────────────────
    public const string SlideshowsView = "slideshows.view";
    public const string SlideshowsInsert = "slideshows.insert";
    public const string SlideshowsEdit = "slideshows.edit";
    public const string SlideshowsDelete = "slideshows.delete";

    // ── Forms & surveys (admin) ─────────────────────────────────────
    public const string FormsView = "forms.view";
    public const string FormsInsert = "forms.insert";
    public const string FormsEdit = "forms.edit";
    public const string FormsDelete = "forms.delete";
    public const string FormsReport = "forms.report";

    // ── Themes (admin) ──────────────────────────────────────────────
    public const string ThemesView = "themes.view";
    public const string ThemesEdit = "themes.edit";

    // ── Content: posts (admin) ──────────────────────────────────────
    public const string PostsView = "posts.view";
    public const string PostsInsert = "posts.insert";
    public const string PostsEdit = "posts.edit";
    public const string PostsDelete = "posts.delete";

    // ── Content: pages (admin) ──────────────────────────────────────
    public const string PagesView = "pages.view";
    public const string PagesInsert = "pages.insert";
    public const string PagesEdit = "pages.edit";
    public const string PagesDelete = "pages.delete";

    // ── Content: taxonomy — categories, tags, post types (admin) ────
    public const string TaxonomyView = "taxonomy.view";
    public const string TaxonomyInsert = "taxonomy.insert";
    public const string TaxonomyEdit = "taxonomy.edit";
    public const string TaxonomyDelete = "taxonomy.delete";

    // ── Website redirects (admin) ───────────────────────────────────
    public const string RedirectsView = "redirects.view";
    public const string RedirectsInsert = "redirects.insert";
    public const string RedirectsEdit = "redirects.edit";
    public const string RedirectsDelete = "redirects.delete";

    // ── Login logs (admin) ──────────────────────────────────────────
    public const string LoginLogsView = "loginlogs.view";

    // ── Catalog: products (admin) ────────────────────────────────────
    public const string ProductsView = "products.view";
    public const string ProductsInsert = "products.insert";
    public const string ProductsEdit = "products.edit";
    public const string ProductsDelete = "products.delete";

    // ── Catalog taxonomy: categories, attributes, brands (admin) ────
    public const string CatalogTaxonomyView = "catalogtaxonomy.view";
    public const string CatalogTaxonomyInsert = "catalogtaxonomy.insert";
    public const string CatalogTaxonomyEdit = "catalogtaxonomy.edit";
    public const string CatalogTaxonomyDelete = "catalogtaxonomy.delete";

    // ── Vendors (admin) ──────────────────────────────────────────────
    public const string VendorsView = "vendors.view";
    public const string VendorsInsert = "vendors.insert";
    public const string VendorsEdit = "vendors.edit";
    public const string VendorsDelete = "vendors.delete";

    // ── Inventory / stock (admin) ────────────────────────────────────
    public const string InventoryView = "inventory.view";
    public const string InventoryEdit = "inventory.edit";

    // ── Warehouse WMS (admin) ─────────────────────────────────────────
    public const string WarehouseView = "warehouse.view";
    public const string WarehouseReceive = "warehouse.receive";
    public const string WarehouseIssue = "warehouse.issue";
    public const string WarehouseApprove = "warehouse.approve";
    public const string WarehousePost = "warehouse.post";

    // ── Suppliers (admin) ─────────────────────────────────────────────
    public const string SuppliersView = "suppliers.view";
    public const string SuppliersInsert = "suppliers.insert";
    public const string SuppliersEdit = "suppliers.edit";
    public const string SuppliersDelete = "suppliers.delete";

    // ── Customer wallets (admin) ─────────────────────────────────────
    public const string WalletsView = "wallets.view";
    public const string WalletsApprove = "wallets.approve";
    public const string WalletsAdjust = "wallets.adjust";

    // ── Coupons (admin) ───────────────────────────────────────────────
    public const string CouponsView = "coupons.view";
    public const string CouponsInsert = "coupons.insert";
    public const string CouponsEdit = "coupons.edit";
    public const string CouponsDelete = "coupons.delete";

    // ── Shipping methods & rates (admin) ─────────────────────────────
    public const string ShippingView = "shipping.view";
    public const string ShippingInsert = "shipping.insert";
    public const string ShippingEdit = "shipping.edit";
    public const string ShippingDelete = "shipping.delete";

    // ── Tax rates (admin) ─────────────────────────────────────────────
    public const string TaxView = "tax.view";
    public const string TaxInsert = "tax.insert";
    public const string TaxEdit = "tax.edit";
    public const string TaxDelete = "tax.delete";

    // ── Settlements (admin) ───────────────────────────────────────────
    public const string SettlementsView = "settlements.view";
    public const string SettlementsEdit = "settlements.edit";

    // ── General ledger / accounting (admin) ───────────────────────────
    public const string AccountingView = "accounting.view";
    public const string AccountingEdit = "accounting.edit";
    public const string AccountingPost = "accounting.post";
    public const string AccountingReport = "accounting.report";

    // ── HR / payroll (admin) ──────────────────────────────────────────
    public const string HrView = "hr.view";
    public const string HrEdit = "hr.edit";
    public const string PayrollView = "payroll.view";
    public const string PayrollRun = "payroll.run";
    public const string PayrollApprove = "payroll.approve";

    // ── Currency master list + per-website rates (admin) ─────────────
    public const string CurrencyView = "currency.view";
    public const string CurrencyEdit = "currency.edit";

    // ── Orders (admin) ────────────────────────────────────────────────
    public const string OrdersView = "orders.view";
    public const string OrdersEdit = "orders.edit";

    // ── Payments & refunds (admin) ────────────────────────────────────
    public const string PaymentsView = "payments.view";
    public const string PaymentsVerify = "payments.verify";
    public const string PaymentsRefund = "payments.refund";

    // ── Website's own bank accounts (admin) ──────────────────────────
    public const string BankAccountsView = "bankaccounts.view";
    public const string BankAccountsInsert = "bankaccounts.insert";
    public const string BankAccountsEdit = "bankaccounts.edit";
    public const string BankAccountsDelete = "bankaccounts.delete";

    // ── Product review / Q&A moderation (admin) ──────────────────────
    public const string ModerationView = "moderation.view";
    public const string ModerationReview = "moderation.review";

    // ── Support desk / Customer 360 (admin) ──────────────────────────
    public const string SupportView = "support.view";
    public const string SupportEdit = "support.edit";

    // ── Staff tasks (admin) ──────────────────────────────────────────
    public const string TasksView = "tasks.view";
    public const string TasksManage = "tasks.manage";

    // ── Client (website customers) — never used in the admin panel ──
    public const string ClientAccess = "client.access";
    public const string ClientPurchase = "client.purchase";
    public const string ClientReview = "client.review";
    public const string ClientProfile = "client.profile";
}
