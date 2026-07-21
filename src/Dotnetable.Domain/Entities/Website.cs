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

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

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

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public virtual ICollection<TaxRate> TaxRates { get; set; } = new List<TaxRate>();

    public virtual ICollection<VendorProduct> VendorProducts { get; set; } = new List<VendorProduct>();

    public virtual ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();

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
