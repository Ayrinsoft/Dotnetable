using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Website
{
    public int WebsiteID { get; set; }

    public string TradeName { get; set; } = null!;

    public string WebsiteAddress { get; set; } = null!;

    public Guid AuthCode { get; set; }

    public bool Active { get; set; }

    public string Manager { get; set; } = null!;

    public string Mobile { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateOnly RegisterDate { get; set; }

    public bool AllowAllIP { get; set; }

    public string DefaultLanguageCode { get; set; } = null!;

    public byte WebsiteType { get; set; }

    public bool IsHub { get; set; }

    /// <summary>
    /// show in title of pages
    /// </summary>
    public string BrandName { get; set; } = null!;

    public int? LogoFileID { get; set; }

    public int? FaveIconFileID { get; set; }

    public string DefaultCurrencyCode { get; set; } = null!;

    /// <summary>
    /// When true, monetary amounts are also persisted in USD columns.
    /// When false (default), site currency is the stored source of truth; USD is derived via rates for multi-currency display and checkout snapshots.
    /// </summary>
    public bool StorePricesInUsd { get; set; }

    /// <summary>When false, checkout does not add tax (TaxTotal stays 0).</summary>
    public bool TaxEnabled { get; set; } = true;

    /// <summary>When true, catalog prices already include tax; checkout extracts or reports tax without adding on top.</summary>
    public bool PricesIncludeTax { get; set; }

    /// <summary>When true, eligible tax rates may apply to shipping as well as merchandise.</summary>
    public bool TaxOnShipping { get; set; }

    /// <summary>Optional default tax jurisdiction country for seller / fallback matching.</summary>
    public int? TaxCountryID { get; set; }

    /// <summary>Seller legal name for invoices / tax reports.</summary>
    public string? SellerLegalName { get; set; }

    /// <summary>Seller TIN / national company ID.</summary>
    public string? SellerTaxId { get; set; }

    /// <summary>Seller economic code (کد اقتصادی).</summary>
    public string? SellerEconomicCode { get; set; }

    /// <summary>Seller VAT / GST number.</summary>
    public string? SellerVatNumber { get; set; }

    /// <summary>Seller commercial registration number.</summary>
    public string? SellerRegistrationNumber { get; set; }

    /// <summary>
    /// When true, checkout may offer cash-on-delivery (pay for goods on delivery) for shipping methods that support COD.
    /// When false, COD shipping options are hidden even if a method has SupportsCod.
    /// </summary>
    public bool AllowCashOnDelivery { get; set; } = true;

    /// <summary>
    /// Default for admin-created / non-online-channel orders: whether to include them in tax filings
    /// (<c>Order.ReportToTax</c>). Offline social sales in some jurisdictions may stay off tax reports.
    /// Online storefront checkout is unaffected (always reportable when tax applies).
    /// </summary>
    public bool ReportOfflineOrdersToTax { get; set; }

    /// <summary>
    /// Site-wide free-shipping threshold in site currency. When cart merchandise subtotal reaches this amount,
    /// shipping quotes are free (0). Zero means no site-wide free shipping (method-level thresholds may still apply).
    /// </summary>
    public decimal FreeShippingMinOrderAmount { get; set; }

    /// <summary>USD dual / conversion bridge for <see cref="FreeShippingMinOrderAmount"/>.</summary>
    public decimal FreeShippingMinOrderAmountUsd { get; set; }

    /// <summary>
    /// 1–3 Latin letters used as the product code prefix for this site (<c>{prefix}-{ProductID}</c>).
    /// Default <c>DN</c>. Normalized via <see cref="Dotnetable.Domain.ProductCode.NormalizePrefix"/>.
    /// </summary>
    public string ProductCodePrefix { get; set; } = "DN";

    /// <summary><see cref="Enums.FiscalPeriodCadence"/> — day / week / month / year accounting periods.</summary>
    public byte FiscalPeriodCadence { get; set; } = 3; // Monthly

    /// <summary>Month (1–12) when the fiscal year starts (used for yearly/monthly generation).</summary>
    public byte FiscalYearStartMonth { get; set; } = 1;

    /// <summary>Day of week for weekly periods: 0 = Sunday … 6 = Saturday.</summary>
    public byte FiscalWeekStartDay { get; set; } = 1; // Monday

    /// <summary>
    /// Days after <c>PeriodTo</c> when the period is due to be closed (soft deadline for the site owner).
    /// </summary>
    public int FiscalCloseDueDays { get; set; } = 5;

    /// <summary>When false, storefront customers cannot open a return request.</summary>
    public bool ReturnsEnabled { get; set; } = true;

    /// <summary>Days after the window start (ship or delivered) when a customer may request a return.</summary>
    public int ReturnWindowDays { get; set; } = 7;

    /// <summary><see cref="Enums.ReturnWindowFrom"/> — 0 = shipped at, 1 = marked delivered.</summary>
    public byte ReturnWindowFrom { get; set; }

    public virtual ICollection<AdminNotification> AdminNotifications { get; set; } = new List<AdminNotification>();

    public virtual ICollection<AttributeDefinition> AttributeDefinitions { get; set; } = new List<AttributeDefinition>();

    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    public virtual ICollection<Bank> Banks { get; set; } = new List<Bank>();

    public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<ChartOfAccount> ChartOfAccounts { get; set; } = new List<ChartOfAccount>();

    public virtual ICollection<ClientBankAccount> ClientBankAccounts { get; set; } = new List<ClientBankAccount>();

    public virtual ICollection<ClientWalletTransaction> ClientWalletTransactions { get; set; } = new List<ClientWalletTransaction>();

    public virtual ICollection<ClientWalletWithdrawal> ClientWalletWithdrawals { get; set; } = new List<ClientWalletWithdrawal>();

    public virtual ICollection<ClientWallet> ClientWallets { get; set; } = new List<ClientWallet>();

    public virtual ICollection<WebsiteWalletCurrency> WebsiteWalletCurrencies { get; set; } = new List<WebsiteWalletCurrency>();

    public virtual ICollection<ContactUsMessage> ContactUsMessages { get; set; } = new List<ContactUsMessage>();

    public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();

    public virtual ICollection<CurrencyRate> CurrencyRates { get; set; } = new List<CurrencyRate>();

    public virtual Currency DefaultCurrencyCodeNavigation { get; set; } = null!;

    public virtual ICollection<EmailAccount> EmailAccounts { get; set; } = new List<EmailAccount>();

    public virtual ICollection<EmailSubscribe> EmailSubscribes { get; set; } = new List<EmailSubscribe>();

    public virtual ICollection<EmailTemplate> EmailTemplates { get; set; } = new List<EmailTemplate>();

    public virtual FileRecord? FaveIconFile { get; set; }

    public virtual ICollection<FileFolder> FileFolders { get; set; } = new List<FileFolder>();

    public virtual ICollection<FileRecord> FileRecords { get; set; } = new List<FileRecord>();

    public virtual ICollection<FileTag> FileTags { get; set; } = new List<FileTag>();

    public virtual ICollection<Form> Forms { get; set; } = new List<Form>();

    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();

    public virtual ICollection<Language> Languages { get; set; } = new List<Language>();

    public virtual ICollection<LocalizationKey> LocalizationKeys { get; set; } = new List<LocalizationKey>();

    public virtual ICollection<LoginTry> LoginTries { get; set; } = new List<LoginTry>();

    public virtual FileRecord? LogoFile { get; set; }

    public virtual ICollection<MediaSet> MediaSets { get; set; } = new List<MediaSet>();

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();

    public virtual ICollection<OrderItem> OrderItemSourceWebsites { get; set; } = new List<OrderItem>();

    public virtual ICollection<OrderItem> OrderItemWebsites { get; set; } = new List<OrderItem>();

    public virtual ICollection<DigitalAccessLog> DigitalAccessLogs { get; set; } = new List<DigitalAccessLog>();

    public virtual ICollection<OrderDigitalAsset> OrderDigitalAssets { get; set; } = new List<OrderDigitalAsset>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<SupportSession> SupportSessions { get; set; } = new List<SupportSession>();

    public virtual ICollection<Page> Pages { get; set; } = new List<Page>();

    public virtual ICollection<PaymentGateway> PaymentGateways { get; set; } = new List<PaymentGateway>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();

    public virtual ICollection<PostType> PostTypes { get; set; } = new List<PostType>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    public virtual ICollection<ProductQuestion> ProductQuestions { get; set; } = new List<ProductQuestion>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Settlement> SettlementTargetWebsites { get; set; } = new List<Settlement>();

    public virtual ICollection<Settlement> SettlementWebsites { get; set; } = new List<Settlement>();

    public virtual ICollection<ShippingMethod> ShippingMethods { get; set; } = new List<ShippingMethod>();

    public virtual ICollection<Slideshow> Slideshows { get; set; } = new List<Slideshow>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();

    public virtual ICollection<Supplier> SupplierLinkedWebsites { get; set; } = new List<Supplier>();

    public virtual Country? TaxCountry { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public virtual ICollection<TaxRate> TaxRates { get; set; } = new List<TaxRate>();

    public virtual ICollection<Vendor> VendorLinkedWebsites { get; set; } = new List<Vendor>();

    public virtual ICollection<VendorCreditTransaction> VendorCreditTransactions { get; set; } = new List<VendorCreditTransaction>();

    public virtual ICollection<VendorProduct> VendorProducts { get; set; } = new List<VendorProduct>();

    public virtual ICollection<Vendor> VendorWebsites { get; set; } = new List<Vendor>();

    public virtual ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();

    public virtual WebsiteCaptchaSetting? WebsiteCaptchaSetting { get; set; }

    public virtual ICollection<WebsiteClient> WebsiteClients { get; set; } = new List<WebsiteClient>();

    public virtual ICollection<WebsiteFeature> WebsiteFeatures { get; set; } = new List<WebsiteFeature>();

    public virtual ICollection<WebsiteIP> WebsiteIPs { get; set; } = new List<WebsiteIP>();

    public virtual ICollection<WebsiteRedirect> WebsiteRedirects { get; set; } = new List<WebsiteRedirect>();

    public virtual ICollection<WebsiteScript> WebsiteScripts { get; set; } = new List<WebsiteScript>();

    public virtual ICollection<WebsiteSeoSetting> WebsiteSeoSettings { get; set; } = new List<WebsiteSeoSetting>();

    public virtual ICollection<WebsiteSocialLink> WebsiteSocialLinks { get; set; } = new List<WebsiteSocialLink>();

    public virtual ICollection<WebsiteStorageSetting> WebsiteStorageSettings { get; set; } = new List<WebsiteStorageSetting>();

    public virtual ICollection<WebsiteTheme> WebsiteThemes { get; set; } = new List<WebsiteTheme>();

    public virtual ICollection<WebsiteWatermarkSetting> WebsiteWatermarkSettings { get; set; } = new List<WebsiteWatermarkSetting>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
