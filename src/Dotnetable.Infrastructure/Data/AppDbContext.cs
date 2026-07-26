using System;
using System.Collections.Generic;
using Dotnetable.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminNotification> AdminNotifications { get; set; }

    public virtual DbSet<AttributeDefinition> AttributeDefinitions { get; set; }

    public virtual DbSet<AttributeDefinitionTranslation> AttributeDefinitionTranslations { get; set; }

    public virtual DbSet<AttributeOption> AttributeOptions { get; set; }

    public virtual DbSet<AttributeOptionTranslation> AttributeOptionTranslations { get; set; }

    public virtual DbSet<Bank> Banks { get; set; }

    public virtual DbSet<BankAccount> BankAccounts { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<BrandTranslation> BrandTranslations { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryTranslation> CategoryTranslations { get; set; }

    public virtual DbSet<ChartOfAccount> ChartOfAccounts { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<CityTranslation> CityTranslations { get; set; }

    public virtual DbSet<ClientBankAccount> ClientBankAccounts { get; set; }

    public virtual DbSet<ClientWallet> ClientWallets { get; set; }

    public virtual DbSet<ClientWalletTransaction> ClientWalletTransactions { get; set; }

    public virtual DbSet<ClientWalletWithdrawal> ClientWalletWithdrawals { get; set; }

    public virtual DbSet<ContactUsMessage> ContactUsMessages { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<CountryTranslation> CountryTranslations { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<CouponRedemption> CouponRedemptions { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<CurrencyRate> CurrencyRates { get; set; }

    public virtual DbSet<EmailAccount> EmailAccounts { get; set; }

    public virtual DbSet<EmailSubscribe> EmailSubscribes { get; set; }

    public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }

    public virtual DbSet<EmailTemplateTranslation> EmailTemplateTranslations { get; set; }

    public virtual DbSet<FileFolder> FileFolders { get; set; }

    public virtual DbSet<FileRecord> FileRecords { get; set; }

    public virtual DbSet<FileRecordTag> FileRecordTags { get; set; }

    public virtual DbSet<FileTag> FileTags { get; set; }

    public virtual DbSet<Form> Forms { get; set; }

    public virtual DbSet<FormField> FormFields { get; set; }

    public virtual DbSet<FormFieldOption> FormFieldOptions { get; set; }

    public virtual DbSet<FormResponse> FormResponses { get; set; }

    public virtual DbSet<FormResponseValue> FormResponseValues { get; set; }

    public virtual DbSet<InventoryItem> InventoryItems { get; set; }

    public virtual DbSet<JournalEntry> JournalEntries { get; set; }

    public virtual DbSet<JournalEntryLine> JournalEntryLines { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<LocalizationKey> LocalizationKeys { get; set; }

    public virtual DbSet<LocalizationValue> LocalizationValues { get; set; }

    public virtual DbSet<LoginTry> LoginTries { get; set; }

    public virtual DbSet<MediaSet> MediaSets { get; set; }

    public virtual DbSet<MediaSetItem> MediaSetItems { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<MemberForgetPassword> MemberForgetPasswords { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MenuItem> MenuItems { get; set; }

    public virtual DbSet<MenuItemTranslation> MenuItemTranslations { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public virtual DbSet<Page> Pages { get; set; }

    public virtual DbSet<PageTranslation> PageTranslations { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentGateway> PaymentGateways { get; set; }

    public virtual DbSet<PaymentRefund> PaymentRefunds { get; set; }

    public virtual DbSet<Policy> Policies { get; set; }

    public virtual DbSet<PolicyRole> PolicyRoles { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostCategory> PostCategories { get; set; }

    public virtual DbSet<PostTranslation> PostTranslations { get; set; }

    public virtual DbSet<PostType> PostTypes { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAnswer> ProductAnswers { get; set; }

    public virtual DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }

    public virtual DbSet<ProductAttributeValueTranslation> ProductAttributeValueTranslations { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductCategoryMap> ProductCategoryMaps { get; set; }

    public virtual DbSet<ProductCategoryRelation> ProductCategoryRelations { get; set; }

    public virtual DbSet<ProductCategoryTranslation> ProductCategoryTranslations { get; set; }

    public virtual DbSet<ProductMedium> ProductMedia { get; set; }

    public virtual DbSet<ProductQuestion> ProductQuestions { get; set; }

    public virtual DbSet<ProductRelation> ProductRelations { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<ProductTranslation> ProductTranslations { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<ProductVariantPriceHistory> ProductVariantPriceHistories { get; set; }

    public virtual DbSet<ProductWarning> ProductWarnings { get; set; }

    public virtual DbSet<ProductWarningTranslation> ProductWarningTranslations { get; set; }

    public virtual DbSet<ProductWarranty> ProductWarranties { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Settlement> Settlements { get; set; }

    public virtual DbSet<SettlementItem> SettlementItems { get; set; }

    public virtual DbSet<ShippingMethod> ShippingMethods { get; set; }

    public virtual DbSet<ShippingRate> ShippingRates { get; set; }

    public virtual DbSet<Slideshow> Slideshows { get; set; }

    public virtual DbSet<SlideshowSlide> SlideshowSlides { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<StateTranslation> StateTranslations { get; set; }

    public virtual DbSet<StockMovement> StockMovements { get; set; }

    public virtual DbSet<SupportInteraction> SupportInteractions { get; set; }

    public virtual DbSet<SupportSession> SupportSessions { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TagTranslation> TagTranslations { get; set; }

    public virtual DbSet<TaxRate> TaxRates { get; set; }

    public virtual DbSet<VariantAttributeValue> VariantAttributeValues { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }

    public virtual DbSet<VendorCreditTransaction> VendorCreditTransactions { get; set; }

    public virtual DbSet<VendorProduct> VendorProducts { get; set; }

    public virtual DbSet<VendorTranslation> VendorTranslations { get; set; }

    public virtual DbSet<Warranty> Warranties { get; set; }

    public virtual DbSet<WarrantyTranslation> WarrantyTranslations { get; set; }

    public virtual DbSet<Website> Websites { get; set; }

    public virtual DbSet<WebsiteCaptchaSetting> WebsiteCaptchaSettings { get; set; }

    public virtual DbSet<WebsiteClient> WebsiteClients { get; set; }

    public virtual DbSet<WebsiteClientAddress> WebsiteClientAddresses { get; set; }

    public virtual DbSet<WebsiteClientForgetPassword> WebsiteClientForgetPasswords { get; set; }

    public virtual DbSet<WebsiteFeature> WebsiteFeatures { get; set; }

    public virtual DbSet<WebsiteIP> WebsiteIPs { get; set; }

    public virtual DbSet<WebsiteRedirect> WebsiteRedirects { get; set; }

    public virtual DbSet<WebsiteScript> WebsiteScripts { get; set; }

    public virtual DbSet<WebsiteSeoSetting> WebsiteSeoSettings { get; set; }

    public virtual DbSet<WebsiteSocialLink> WebsiteSocialLinks { get; set; }

    public virtual DbSet<WebsiteStorageSetting> WebsiteStorageSettings { get; set; }

    public virtual DbSet<WebsiteTheme> WebsiteThemes { get; set; }

    public virtual DbSet<WebsiteWatermarkSetting> WebsiteWatermarkSettings { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    public virtual DbSet<WishlistItem> WishlistItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminNotification>(entity =>
        {
            entity.HasIndex(e => new { e.MemberID, e.IsRead, e.CreatedAt }, "IX_AdminNotifications_MemberID_IsRead_CreatedAt").IsDescending(false, false, true);

            entity.HasIndex(e => e.WebsiteID, "IX_AdminNotifications_WebsiteID");

            entity.Property(e => e.ActionUrl)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Member).WithMany(p => p.AdminNotifications)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AdminNotifications_Members");

            entity.HasOne(d => d.Website).WithMany(p => p.AdminNotifications)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AdminNotifications_Websites");
        });

        modelBuilder.Entity<AttributeDefinition>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_AttributeDefinitions_WebsiteID");

            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Unit).HasMaxLength(30);

            entity.HasOne(d => d.Website).WithMany(p => p.AttributeDefinitions)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttributeDefinitions_Websites");
        });

        modelBuilder.Entity<AttributeDefinitionTranslation>(entity =>
        {
            entity.HasIndex(e => e.AttributeDefinitionID, "IX_AttributeDefinitionTranslations_AttributeDefinitionID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Unit).HasMaxLength(30);

            entity.HasOne(d => d.AttributeDefinition).WithMany(p => p.AttributeDefinitionTranslations)
                .HasForeignKey(d => d.AttributeDefinitionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttributeDefinitionTranslations_AttributeDefinitions");
        });

        modelBuilder.Entity<AttributeOption>(entity =>
        {
            entity.HasIndex(e => e.AttributeDefinitionID, "IX_AttributeOptions_AttributeDefinitionID");

            entity.Property(e => e.ColorHex)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Value).HasMaxLength(2000);

            entity.HasOne(d => d.AttributeDefinition).WithMany(p => p.AttributeOptions)
                .HasForeignKey(d => d.AttributeDefinitionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttributeOptions_AttributeDefinitions");
        });

        modelBuilder.Entity<AttributeOptionTranslation>(entity =>
        {
            entity.HasIndex(e => e.AttributeOptionID, "IX_AttributeOptionTranslations_AttributeOptionID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Value).HasMaxLength(2000);

            entity.HasOne(d => d.AttributeOption).WithMany(p => p.AttributeOptionTranslations)
                .HasForeignKey(d => d.AttributeOptionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttributeOptionTranslations_AttributeOptions");
        });

        modelBuilder.Entity<Bank>(entity =>
        {
            entity.HasIndex(e => e.LogoFileID, "IX_Banks_LogoFileID");

            entity.HasIndex(e => e.WebsiteID, "IX_Banks_WebsiteID");

            entity.Property(e => e.BankCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(64);

            entity.HasOne(d => d.LogoFile).WithMany(p => p.Banks)
                .HasForeignKey(d => d.LogoFileID)
                .HasConstraintName("FK_Banks_FileRecords");

            entity.HasOne(d => d.Website).WithMany(p => p.Banks)
                .HasForeignKey(d => d.WebsiteID)
                .HasConstraintName("FK_Banks_Websites");
        });

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasIndex(e => e.BankID, "IX_BankAccounts_BankID");

            entity.HasIndex(e => e.CreatedByMemberId, "IX_BankAccounts_CreatedByMemberId");

            entity.HasIndex(e => e.WebsiteID, "IX_BankAccounts_WebsiteID");

            entity.Property(e => e.AccountNumber)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CardNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.IBAN)
                .HasMaxLength(34)
                .IsUnicode(false);
            entity.Property(e => e.OwnerName).HasMaxLength(90);
            entity.Property(e => e.Title).HasMaxLength(70);

            entity.HasOne(d => d.Bank).WithMany(p => p.BankAccounts)
                .HasForeignKey(d => d.BankID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BankAccounts_Banks");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.BankAccounts)
                .HasForeignKey(d => d.CreatedByMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BankAccounts_Members");

            entity.HasOne(d => d.Website).WithMany(p => p.BankAccounts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BankAccounts_Websites");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasIndex(e => e.LogoFileID, "IX_Brands_LogoFileID");

            entity.HasIndex(e => e.WebsiteID, "IX_Brands_WebsiteID");

            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);

            entity.HasOne(d => d.LogoFile).WithMany(p => p.Brands)
                .HasForeignKey(d => d.LogoFileID)
                .HasConstraintName("FK_Brands_FileRecords");

            entity.HasOne(d => d.Website).WithMany(p => p.Brands)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Brands_Websites");
        });

        modelBuilder.Entity<BrandTranslation>(entity =>
        {
            entity.HasIndex(e => e.BrandID, "IX_BrandTranslations_BrandID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);

            entity.HasOne(d => d.Brand).WithMany(p => p.BrandTranslations)
                .HasForeignKey(d => d.BrandID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BrandTranslations_Brands");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasIndex(e => e.CouponID, "IX_Carts_CouponID");

            entity.HasIndex(e => e.WebsiteClientID, "IX_Carts_WebsiteClientID");

            entity.HasIndex(e => e.WebsiteID, "IX_Carts_WebsiteID");

            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.SessionKey)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.Coupon).WithMany(p => p.Carts)
                .HasForeignKey(d => d.CouponID)
                .HasConstraintName("FK_Carts_Coupons");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.Carts)
                .HasForeignKey(d => d.WebsiteClientID)
                .HasConstraintName("FK_Carts_WebsiteClients");

            entity.HasOne(d => d.Website).WithMany(p => p.Carts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Carts_Websites");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(e => e.ProductVariantID, "IX_CartItems_ProductVariantID");

            entity.HasIndex(e => e.VendorProductID, "IX_CartItems_VendorProductID");

            entity.HasIndex(e => new { e.CartID, e.ProductVariantID, e.VendorProductID }, "UQ_CartItems_CartID_ProductVariantID_VendorProductID").IsUnique();

            entity.Property(e => e.AddedAt).HasPrecision(0);

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartItems_Carts");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductVariantID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartItems_ProductVariants");

            entity.HasOne(d => d.VendorProduct).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.VendorProductID)
                .HasConstraintName("FK_CartItems_VendorProducts");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.ParentCategoryID, "IX_Categories_ParentCategoryID");

            entity.HasIndex(e => e.PostTypeID, "IX_Categories_PostTypeID");

            entity.HasIndex(e => e.WebsiteID, "IX_Categories_WebsiteID");

            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryID)
                .HasConstraintName("FK_Categories_Categories");

            entity.HasOne(d => d.PostType).WithMany(p => p.Categories)
                .HasForeignKey(d => d.PostTypeID)
                .HasConstraintName("FK_Categories_PostTypes");

            entity.HasOne(d => d.Website).WithMany(p => p.Categories)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Categories_Websites");
        });

        modelBuilder.Entity<CategoryTranslation>(entity =>
        {
            entity.HasIndex(e => e.CategoryID, "IX_CategoryTranslations_CategoryID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryTranslations)
                .HasForeignKey(d => d.CategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoryTranslations_Categories");
        });

        modelBuilder.Entity<ChartOfAccount>(entity =>
        {
            entity.HasIndex(e => e.ParentAccountID, "IX_ChartOfAccounts_ParentAccountID");

            entity.HasIndex(e => e.WebsiteID, "IX_ChartOfAccounts_WebsiteID");

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.ParentAccount).WithMany(p => p.InverseParentAccount)
                .HasForeignKey(d => d.ParentAccountID)
                .HasConstraintName("FK_ChartOfAccounts_ChartOfAccounts");

            entity.HasOne(d => d.Website).WithMany(p => p.ChartOfAccounts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChartOfAccounts_Websites");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasIndex(e => e.CountryID, "IX_Cities_CountryID");

            entity.HasIndex(e => e.StateID, "IX_Cities_StateID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(48);

            entity.HasOne(d => d.Country).WithMany(p => p.Cities)
                .HasForeignKey(d => d.CountryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cities_Countries");

            entity.HasOne(d => d.State).WithMany(p => p.Cities)
                .HasForeignKey(d => d.StateID)
                .HasConstraintName("FK_Cities_States");
        });

        modelBuilder.Entity<CityTranslation>(entity =>
        {
            entity.HasIndex(e => e.CityID, "IX_CityTranslations_CityID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(48);

            entity.HasOne(d => d.City).WithMany(p => p.CityTranslations)
                .HasForeignKey(d => d.CityID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CityTranslations_Cities");
        });

        modelBuilder.Entity<ClientBankAccount>(entity =>
        {
            entity.HasIndex(e => e.BankID, "IX_ClientBankAccounts_BankID");

            entity.HasIndex(e => e.WebsiteClientID, "IX_ClientBankAccounts_WebsiteClientID");

            entity.HasIndex(e => e.WebsiteID, "IX_ClientBankAccounts_WebsiteID");

            entity.Property(e => e.AccountNumber)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CardNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.IBAN)
                .HasMaxLength(34)
                .IsUnicode(false);
            entity.Property(e => e.OwnerName).HasMaxLength(90);

            entity.HasOne(d => d.Bank).WithMany(p => p.ClientBankAccounts)
                .HasForeignKey(d => d.BankID)
                .HasConstraintName("FK_ClientBankAccounts_Banks");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.ClientBankAccounts)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientBankAccounts_WebsiteClients");

            entity.HasOne(d => d.Website).WithMany(p => p.ClientBankAccounts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientBankAccounts_Websites");
        });

        modelBuilder.Entity<ClientWallet>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_ClientWallets_WebsiteID");

            entity.HasIndex(e => e.WebsiteClientID, "UQ_ClientWallets_WebsiteClientID").IsUnique();

            entity.Property(e => e.Balance).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.BalanceUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.WebsiteClient).WithOne(p => p.ClientWallet)
                .HasForeignKey<ClientWallet>(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientWallets_WebsiteClients");

            entity.HasOne(d => d.Website).WithMany(p => p.ClientWallets)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientWallets_Websites");
        });

        modelBuilder.Entity<ClientWalletTransaction>(entity =>
        {
            entity.HasIndex(e => e.ClientWalletID, "IX_ClientWalletTransactions_ClientWalletID");

            entity.HasIndex(e => e.CreatedByMemberID, "IX_ClientWalletTransactions_CreatedByMemberID");

            entity.HasIndex(e => e.WebsiteID, "IX_ClientWalletTransactions_WebsiteID");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.AmountUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.BalanceAfter).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.BalanceAfterUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.Note).HasMaxLength(500);

            entity.HasOne(d => d.ClientWallet).WithMany(p => p.ClientWalletTransactions)
                .HasForeignKey(d => d.ClientWalletID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientWalletTransactions_ClientWallets");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.ClientWalletTransactions)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_ClientWalletTransactions_Members");

            entity.HasOne(d => d.Website).WithMany(p => p.ClientWalletTransactions)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientWalletTransactions_Websites");
        });

        modelBuilder.Entity<ClientWalletWithdrawal>(entity =>
        {
            entity.HasIndex(e => e.ClientBankAccountID, "IX_ClientWalletWithdrawals_ClientBankAccountID");

            entity.HasIndex(e => e.ClientWalletID, "IX_ClientWalletWithdrawals_ClientWalletID");

            entity.HasIndex(e => e.ReviewedByMemberID, "IX_ClientWalletWithdrawals_ReviewedByMemberID");

            entity.HasIndex(e => e.WebsiteClientID, "IX_ClientWalletWithdrawals_WebsiteClientID");

            entity.HasIndex(e => e.WebsiteID, "IX_ClientWalletWithdrawals_WebsiteID");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.AmountUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.PaidAt).HasPrecision(0);
            entity.Property(e => e.PaymentRefNumber).HasMaxLength(100);
            entity.Property(e => e.RejectReason).HasMaxLength(500);
            entity.Property(e => e.RequestedAt).HasPrecision(0);
            entity.Property(e => e.ReviewedAt).HasPrecision(0);

            entity.HasOne(d => d.ClientBankAccount).WithMany(p => p.ClientWalletWithdrawals)
                .HasForeignKey(d => d.ClientBankAccountID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientWalletWithdrawals_ClientBankAccounts");

            entity.HasOne(d => d.ClientWallet).WithMany(p => p.ClientWalletWithdrawals)
                .HasForeignKey(d => d.ClientWalletID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientWalletWithdrawals_ClientWallets");

            entity.HasOne(d => d.ReviewedByMember).WithMany(p => p.ClientWalletWithdrawals)
                .HasForeignKey(d => d.ReviewedByMemberID)
                .HasConstraintName("FK_ClientWalletWithdrawals_Members");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.ClientWalletWithdrawals)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientWalletWithdrawals_WebsiteClients");

            entity.HasOne(d => d.Website).WithMany(p => p.ClientWalletWithdrawals)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientWalletWithdrawals_Websites");
        });

        modelBuilder.Entity<ContactUsMessage>(entity =>
        {
            entity.HasKey(e => e.ContactUsMessagesID);

            entity.HasIndex(e => e.WebsiteID, "IX_ContactUsMessages_WebsiteID");

            entity.Property(e => e.CellphoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.EmailAddress)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.LogTime).HasColumnType("datetime");
            entity.Property(e => e.MessageBody).HasMaxLength(4000);
            entity.Property(e => e.MessageSubject).HasMaxLength(512);
            entity.Property(e => e.SenderIPAddress)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SenderName).HasMaxLength(64);

            entity.HasOne(d => d.Website).WithMany(p => p.ContactUsMessages)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContactUsMessages_Websites");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PhonePerfix)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(42);
        });

        modelBuilder.Entity<CountryTranslation>(entity =>
        {
            entity.HasIndex(e => e.CountryID, "IX_CountryTranslations_CountryID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(42);

            entity.HasOne(d => d.Country).WithMany(p => p.CountryTranslations)
                .HasForeignKey(d => d.CountryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CountryTranslations_Countries");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasIndex(e => e.CreatedByMemberID, "IX_Coupons_CreatedByMemberID");

            entity.HasIndex(e => new { e.WebsiteID, e.Code }, "UQ_Coupons_WebsiteID_Code").IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.EndsAt).HasPrecision(0);
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.MaxDiscountAmountUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.MinOrderAmount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.MinOrderAmountUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.StartsAt).HasPrecision(0);

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.Coupons)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_Coupons_Members");

            entity.HasOne(d => d.Website).WithMany(p => p.Coupons)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Coupons_Websites");
        });

        modelBuilder.Entity<CouponRedemption>(entity =>
        {
            entity.HasIndex(e => e.CouponID, "IX_CouponRedemptions_CouponID");

            entity.HasIndex(e => e.WebsiteClientID, "IX_CouponRedemptions_WebsiteClientID");

            entity.HasIndex(e => e.OrderID, "UQ_CouponRedemptions_OrderID").IsUnique();

            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.DiscountAmountUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.RedeemedAt).HasPrecision(0);

            entity.HasOne(d => d.Coupon).WithMany(p => p.CouponRedemptions)
                .HasForeignKey(d => d.CouponID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CouponRedemptions_Coupons");

            entity.HasOne(d => d.Order).WithOne(p => p.CouponRedemption)
                .HasForeignKey<CouponRedemption>(d => d.OrderID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CouponRedemptions_Orders");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.CouponRedemptions)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CouponRedemptions_WebsiteClients");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.CurrencyCode);

            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Symbol).HasMaxLength(10);
        });

        modelBuilder.Entity<CurrencyRate>(entity =>
        {
            entity.HasIndex(e => e.CurrencyCode, "IX_CurrencyRates_CurrencyCode");

            entity.HasIndex(e => e.WebsiteID, "IX_CurrencyRates_WebsiteID");

            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LastUpdate).HasColumnType("datetime");
            entity.Property(e => e.USDToCurrency).HasColumnType("decimal(18, 6)");

            entity.HasOne(d => d.CurrencyCodeNavigation).WithMany(p => p.CurrencyRates)
                .HasForeignKey(d => d.CurrencyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CurrencyRates_Currencies");

            entity.HasOne(d => d.Website).WithMany(p => p.CurrencyRates)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CurrencyRates_Websites");
        });

        modelBuilder.Entity<EmailAccount>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_EmailAccounts_WebsiteID");

            entity.Property(e => e.EmailAddress)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.MailName).HasMaxLength(64);
            entity.Property(e => e.MailServer)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.Property(e => e.Password).HasMaxLength(256);

            entity.HasOne(d => d.Website).WithMany(p => p.EmailAccounts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmailAccounts_Websites");
        });

        modelBuilder.Entity<EmailSubscribe>(entity =>
        {
            entity.HasIndex(e => e.MemberID, "IX_EmailSubscribes_MemberID");

            entity.HasIndex(e => e.WebsiteID, "IX_EmailSubscribes_WebsiteID");

            entity.Property(e => e.Email)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.LogTime).HasColumnType("datetime");

            entity.HasOne(d => d.Member).WithMany(p => p.EmailSubscribes)
                .HasForeignKey(d => d.MemberID)
                .HasConstraintName("FK_EmailSubscribes_Members");

            entity.HasOne(d => d.Website).WithMany(p => p.EmailSubscribes)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmailSubscribes_Websites");
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasIndex(e => new { e.WebsiteID, e.TemplateKey }, "IX_EmailTemplates_WebsiteID_TemplateKey").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.Subject).HasMaxLength(256);
            entity.Property(e => e.TemplateKey)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.HasOne(d => d.Website).WithMany(p => p.EmailTemplates)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmailTemplates_Websites");
        });

        modelBuilder.Entity<EmailTemplateTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.EmailTemplateID, e.LanguageCode }, "IX_EmailTemplateTranslations_EmailTemplateID_LanguageCode").IsUnique();

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Subject).HasMaxLength(256);

            entity.HasOne(d => d.EmailTemplate).WithMany(p => p.EmailTemplateTranslations)
                .HasForeignKey(d => d.EmailTemplateID)
                .HasConstraintName("FK_EmailTemplateTranslations_EmailTemplates");
        });

        modelBuilder.Entity<FileFolder>(entity =>
        {
            entity.HasIndex(e => e.ParentFolderID, "IX_FileFolders_ParentFolderID");

            entity.HasIndex(e => e.WebsiteID, "IX_FileFolders_WebsiteID");

            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(400);
            entity.Property(e => e.Name).HasMaxLength(120);

            entity.HasOne(d => d.ParentFolder).WithMany(p => p.InverseParentFolder)
                .HasForeignKey(d => d.ParentFolderID)
                .HasConstraintName("FK_FileFolders_Parent");

            entity.HasOne(d => d.Website).WithMany(p => p.FileFolders)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileFolders_Websites");
        });

        modelBuilder.Entity<FileRecord>(entity =>
        {
            entity.HasIndex(e => e.FileFolderID, "IX_FileRecords_FileFolderID");

            entity.HasIndex(e => e.UploaderMemberID, "IX_FileRecords_UploaderMemberID");

            entity.HasIndex(e => e.WebsiteClientID, "IX_FileRecords_WebsiteClientID");

            entity.HasIndex(e => e.WebsiteID, "IX_FileRecords_WebsiteID");

            entity.HasIndex(e => e.WebsiteStorageSettingsID, "IX_FileRecords_WebsiteStorageSettingsID");

            entity.Property(e => e.AltText).HasMaxLength(120);
            entity.Property(e => e.CDNFileCode)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.CNDUrl).HasMaxLength(450);
            entity.Property(e => e.MetadataJSON).HasMaxLength(2000);
            entity.Property(e => e.MimeType)
                .HasMaxLength(74)
                .IsUnicode(false);
            entity.Property(e => e.OriginalFileName).HasMaxLength(120);
            entity.Property(e => e.StoragePath).HasMaxLength(350);
            entity.Property(e => e.StoredFileName)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.ThumbnailCDN).HasMaxLength(450);
            entity.Property(e => e.ThumbnailStorage).HasMaxLength(350);
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.UploadDate).HasColumnType("datetime");

            entity.HasOne(d => d.FileFolder).WithMany(p => p.FileRecords)
                .HasForeignKey(d => d.FileFolderID)
                .HasConstraintName("FK_FileRecords_FileFolders");

            entity.HasOne(d => d.UploaderMember).WithMany(p => p.FileRecords)
                .HasForeignKey(d => d.UploaderMemberID)
                .HasConstraintName("FK_FileRecords_Members");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.FileRecords)
                .HasForeignKey(d => d.WebsiteClientID)
                .HasConstraintName("FK_FileRecords_WebsiteClients");

            entity.HasOne(d => d.Website).WithMany(p => p.FileRecords)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileRecords_Websites");

            entity.HasOne(d => d.WebsiteStorageSettings).WithMany(p => p.FileRecords)
                .HasForeignKey(d => d.WebsiteStorageSettingsID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileRecords_WebsiteStorageSettings");
        });

        modelBuilder.Entity<FileRecordTag>(entity =>
        {
            entity.HasIndex(e => e.FileRecordID, "IX_FileRecordTags_FileRecordID");

            entity.HasIndex(e => e.FileTagID, "IX_FileRecordTags_FileTagID");

            entity.HasOne(d => d.FileRecord).WithMany(p => p.FileRecordTags)
                .HasForeignKey(d => d.FileRecordID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileRecordTags_FileRecords");

            entity.HasOne(d => d.FileTag).WithMany(p => p.FileRecordTags)
                .HasForeignKey(d => d.FileTagID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileRecordTags_FileTags");
        });

        modelBuilder.Entity<FileTag>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_FileTags_WebsiteID");

            entity.Property(e => e.Name).HasMaxLength(60);

            entity.HasOne(d => d.Website).WithMany(p => p.FileTags)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileTags_Websites");
        });

        modelBuilder.Entity<Form>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_Forms_WebsiteID");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EndAt).HasColumnType("datetime");
            entity.Property(e => e.NotifyEmail).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.StartAt).HasColumnType("datetime");
            entity.Property(e => e.SubmitButtonText).HasMaxLength(100);
            entity.Property(e => e.SuccessMessage).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Website).WithMany(p => p.Forms)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Forms_Websites");
        });

        modelBuilder.Entity<FormField>(entity =>
        {
            entity.HasIndex(e => e.FormID, "IX_FormFields_FormID");

            entity.Property(e => e.HelpText).HasMaxLength(500);
            entity.Property(e => e.Label).HasMaxLength(300);
            entity.Property(e => e.Placeholder).HasMaxLength(200);

            entity.HasOne(d => d.Form).WithMany(p => p.FormFields)
                .HasForeignKey(d => d.FormID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FormFields_Forms");
        });

        modelBuilder.Entity<FormFieldOption>(entity =>
        {
            entity.HasIndex(e => e.FormFieldID, "IX_FormFieldOptions_FormFieldID");

            entity.Property(e => e.Label).HasMaxLength(300);
            entity.Property(e => e.Value).HasMaxLength(200);

            entity.HasOne(d => d.FormField).WithMany(p => p.FormFieldOptions)
                .HasForeignKey(d => d.FormFieldID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FormFieldOptions_FormFields");
        });

        modelBuilder.Entity<FormResponse>(entity =>
        {
            entity.HasIndex(e => e.FormID, "IX_FormResponses_FormID");

            entity.HasIndex(e => e.WebsiteClientID, "IX_FormResponses_WebsiteClientID");

            entity.Property(e => e.SenderIPAddress)
                .HasMaxLength(45)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Form).WithMany(p => p.FormResponses)
                .HasForeignKey(d => d.FormID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FormResponses_Forms");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.FormResponses)
                .HasForeignKey(d => d.WebsiteClientID)
                .HasConstraintName("FK_FormResponses_WebsiteClients");
        });

        modelBuilder.Entity<FormResponseValue>(entity =>
        {
            entity.HasIndex(e => e.FormFieldID, "IX_FormResponseValues_FormFieldID");

            entity.HasIndex(e => e.FormResponseID, "IX_FormResponseValues_FormResponseID");

            entity.HasOne(d => d.FormField).WithMany(p => p.FormResponseValues)
                .HasForeignKey(d => d.FormFieldID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FormResponseValues_FormFields");

            entity.HasOne(d => d.FormResponse).WithMany(p => p.FormResponseValues)
                .HasForeignKey(d => d.FormResponseID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FormResponseValues_FormResponses");
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasIndex(e => e.ProductVariantID, "IX_InventoryItems_ProductVariantID");

            entity.HasIndex(e => e.WebsiteID, "IX_InventoryItems_WebsiteID");

            entity.Property(e => e.AvgCost).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.AvgCostUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.InventoryItems)
                .HasForeignKey(d => d.ProductVariantID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryItems_ProductVariants");

            entity.HasOne(d => d.Website).WithMany(p => p.InventoryItems)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryItems_Websites");
        });

        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_JournalEntries_WebsiteID");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EntryNumber).HasMaxLength(30);

            entity.HasOne(d => d.Website).WithMany(p => p.JournalEntries)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalEntries_Websites");
        });

        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.HasIndex(e => e.ChartOfAccountID, "IX_JournalEntryLines_ChartOfAccountID");

            entity.HasIndex(e => e.JournalEntryID, "IX_JournalEntryLines_JournalEntryID");

            entity.Property(e => e.Credit).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Debit).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Description).HasMaxLength(300);

            entity.HasOne(d => d.ChartOfAccount).WithMany(p => p.JournalEntryLines)
                .HasForeignKey(d => d.ChartOfAccountID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalEntryLines_ChartOfAccounts");

            entity.HasOne(d => d.JournalEntry).WithMany(p => p.JournalEntryLines)
                .HasForeignKey(d => d.JournalEntryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalEntryLines_JournalEntries");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasIndex(e => e.LanguageCode, "UQ_Languages_Admin_LanguageCode")
                .IsUnique()
                .HasFilter("([WebsiteID] IS NULL)");

            entity.HasIndex(e => new { e.WebsiteID, e.LanguageCode }, "UQ_Languages_WebsiteID_LanguageCode")
                .IsUnique()
                .HasFilter("([WebsiteID] IS NOT NULL)");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LanguageCodeISO)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(32);

            entity.HasOne(d => d.Website).WithMany(p => p.Languages)
                .HasForeignKey(d => d.WebsiteID)
                .HasConstraintName("FK_Languages_Websites");
        });

        modelBuilder.Entity<LocalizationKey>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_LocalizationKeys_WebsiteID");

            entity.HasIndex(e => e.ItemKey, "UQ_LocalizationKeys_Admin_ItemKey")
                .IsUnique()
                .HasFilter("([WebsiteID] IS NULL)");

            entity.HasIndex(e => new { e.WebsiteID, e.ItemKey }, "UQ_LocalizationKeys_WebsiteID_ItemKey")
                .IsUnique()
                .HasFilter("([WebsiteID] IS NOT NULL)");

            entity.Property(e => e.DefaultValue).HasMaxLength(2000);
            entity.Property(e => e.ItemKey)
                .HasMaxLength(72)
                .IsUnicode(false);

            entity.HasOne(d => d.Website).WithMany(p => p.LocalizationKeys)
                .HasForeignKey(d => d.WebsiteID)
                .HasConstraintName("FK_LocalizationKeys_Websites");
        });

        modelBuilder.Entity<LocalizationValue>(entity =>
        {
            entity.HasIndex(e => e.LocalizationKeyID, "IX_LocalizationValues_LocalizationKeyID");

            entity.Property(e => e.ItemValue).HasMaxLength(2000);
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.LocalizationKey).WithMany(p => p.LocalizationValues)
                .HasForeignKey(d => d.LocalizationKeyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LocalizationValues_LocalizationKeys");
        });

        modelBuilder.Entity<LoginTry>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_LoginTries_WebsiteID");

            entity.Property(e => e.LogTime).HasColumnType("datetime");
            entity.Property(e => e.TryIP)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.HasOne(d => d.Website).WithMany(p => p.LoginTries)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoginTries_Websites");
        });

        modelBuilder.Entity<MediaSet>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_MediaSets_WebsiteID");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Website).WithMany(p => p.MediaSets)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MediaSets_Websites");
        });

        modelBuilder.Entity<MediaSetItem>(entity =>
        {
            entity.HasIndex(e => e.FileID, "IX_MediaSetItems_FileID");

            entity.HasIndex(e => e.MediaSetID, "IX_MediaSetItems_MediaSetID");

            entity.HasIndex(e => e.VideoThumbnailFileID, "IX_MediaSetItems_VideoThumbnailFileID");

            entity.Property(e => e.ExternalVideoUrl).HasMaxLength(500);

            entity.HasOne(d => d.File).WithMany(p => p.MediaSetItemFiles)
                .HasForeignKey(d => d.FileID)
                .HasConstraintName("FK_MediaSetItems_FileRecords");

            entity.HasOne(d => d.MediaSet).WithMany(p => p.MediaSetItems)
                .HasForeignKey(d => d.MediaSetID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MediaSetItems_MediaSets");

            entity.HasOne(d => d.VideoThumbnailFile).WithMany(p => p.MediaSetItemVideoThumbnailFiles)
                .HasForeignKey(d => d.VideoThumbnailFileID)
                .HasConstraintName("FK_MediaSetItems_FileRecord1");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasIndex(e => e.AvatarID, "IX_Members_AvatarID");

            entity.HasIndex(e => e.PolicyID, "IX_Members_PolicyID");

            entity.HasIndex(e => e.WebsiteID, "IX_Members_WebsiteID");

            entity.Property(e => e.AdminUIMode).HasComment("0 = Basic (admin surface reduced to what this member's Website.WebsiteType needs), 1 = General (same admin surface regardless of website type), 2 = Advanced (full surface, e.g. extra-language pages/buttons on an otherwise single-language site)");
            entity.Property(e => e.CellphoneNumber)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.CountryCode)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Givenname).HasMaxLength(64);
            entity.Property(e => e.Password)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Surname).HasMaxLength(64);
            entity.Property(e => e.Username)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.HasOne(d => d.Avatar).WithMany(p => p.Members)
                .HasForeignKey(d => d.AvatarID)
                .HasConstraintName("FK_Members_FileRecords");

            entity.HasOne(d => d.Policy).WithMany(p => p.Members)
                .HasForeignKey(d => d.PolicyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Members_Policies");

            entity.HasOne(d => d.Website).WithMany(p => p.Members)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Members_Websites");
        });

        modelBuilder.Entity<MemberForgetPassword>(entity =>
        {
            entity.HasIndex(e => e.MemberID, "IX_MemberForgetPasswords_MemberID");

            entity.Property(e => e.ForgetKey)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.LogTime).HasColumnType("datetime");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberForgetPasswords)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MemberForgetPasswords_Members");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_Menus_WebsiteID");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Website).WithMany(p => p.Menus)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Menus_Websites");
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasIndex(e => e.BrandID, "IX_MenuItems_BrandID");

            entity.HasIndex(e => e.CategoryID, "IX_MenuItems_CategoryID");

            entity.HasIndex(e => e.MenuID, "IX_MenuItems_MenuID");

            entity.HasIndex(e => e.PageID, "IX_MenuItems_PageID");

            entity.HasIndex(e => e.ParentItemID, "IX_MenuItems_ParentItemID");

            entity.HasIndex(e => e.PostID, "IX_MenuItems_PostID");

            entity.HasIndex(e => e.ProductCategoryID, "IX_MenuItems_ProductCategoryID");

            entity.HasIndex(e => e.ProductID, "IX_MenuItems_ProductID");

            entity.HasIndex(e => e.VendorID, "IX_MenuItems_VendorID");

            entity.Property(e => e.CssClass).HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Url).HasMaxLength(500);

            entity.HasOne(d => d.Brand).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.BrandID)
                .HasConstraintName("FK_MenuItems_Brands");

            entity.HasOne(d => d.Category).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.CategoryID)
                .HasConstraintName("FK_MenuItems_Categories");

            entity.HasOne(d => d.Menu).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.MenuID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuItems_Menus");

            entity.HasOne(d => d.Page).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.PageID)
                .HasConstraintName("FK_MenuItems_Pages");

            entity.HasOne(d => d.ParentItem).WithMany(p => p.InverseParentItem)
                .HasForeignKey(d => d.ParentItemID)
                .HasConstraintName("FK_MenuItems_MenuItems");

            entity.HasOne(d => d.Post).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.PostID)
                .HasConstraintName("FK_MenuItems_Posts");

            entity.HasOne(d => d.ProductCategory).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.ProductCategoryID)
                .HasConstraintName("FK_MenuItems_ProductCategories");

            entity.HasOne(d => d.Product).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.ProductID)
                .HasConstraintName("FK_MenuItems_Products");

            entity.HasOne(d => d.Vendor).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.VendorID)
                .HasConstraintName("FK_MenuItems_Vendors");
        });

        modelBuilder.Entity<MenuItemTranslation>(entity =>
        {
            entity.HasIndex(e => e.MenuItemID, "IX_MenuItemTranslations_MenuItemID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.MenuItem).WithMany(p => p.MenuItemTranslations)
                .HasForeignKey(d => d.MenuItemID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuItemTranslations_MenuItems");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.CouponID, "IX_Orders_CouponID");

            entity.HasIndex(e => e.CreatedByMemberID, "IX_Orders_CreatedByMemberID");

            entity.HasIndex(e => e.CurrencyCode, "IX_Orders_CurrencyCode");

            entity.HasIndex(e => e.ShippingMethodID, "IX_Orders_ShippingMethodID");

            entity.HasIndex(e => e.WebsiteClientAddressID, "IX_Orders_WebsiteClientAddressID");

            entity.HasIndex(e => e.WebsiteClientID, "IX_Orders_WebsiteClientID");

            entity.HasIndex(e => e.WebsiteID, "IX_Orders_WebsiteID");

            entity.Property(e => e.AddressSnapshot).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DiscountTotal).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ExchangeRateToUsd).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.GrandTotal).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.GrandTotalUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.OrderNumber).HasMaxLength(30);
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.ShippingTotal).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.TaxTotal).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.Coupon).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CouponID)
                .HasConstraintName("FK_Orders_Coupons");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_Orders_Members");

            entity.HasOne(d => d.CurrencyCodeNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CurrencyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Currencies");

            entity.HasOne(d => d.ShippingMethod).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ShippingMethodID)
                .HasConstraintName("FK_Orders_ShippingMethods");

            entity.HasOne(d => d.WebsiteClientAddress).WithMany(p => p.Orders)
                .HasForeignKey(d => d.WebsiteClientAddressID)
                .HasConstraintName("FK_Orders_WebsiteClientAddresses");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.Orders)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_WebsiteClients");

            entity.HasOne(d => d.Website).WithMany(p => p.Orders)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Websites");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(e => e.OrderID, "IX_OrderItems_OrderID");

            entity.HasIndex(e => e.ProductVariantID, "IX_OrderItems_ProductVariantID");

            entity.HasIndex(e => e.SourceWebsiteID, "IX_OrderItems_SourceWebsiteID");

            entity.HasIndex(e => e.VendorID, "IX_OrderItems_VendorID");

            entity.HasIndex(e => e.VendorProductID, "IX_OrderItems_VendorProductID");

            entity.HasIndex(e => e.WebsiteID, "IX_OrderItems_WebsiteID");

            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.SkuSnapshot).HasMaxLength(100);
            entity.Property(e => e.TitleSnapshot).HasMaxLength(300);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.UnitCostUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.UnitPriceUsd).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Orders");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductVariantID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_ProductVariants");

            entity.HasOne(d => d.SourceWebsite).WithMany(p => p.OrderItemSourceWebsites)
                .HasForeignKey(d => d.SourceWebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Website1");

            entity.HasOne(d => d.Vendor).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.VendorID)
                .HasConstraintName("FK_OrderItems_Vendors");

            entity.HasOne(d => d.VendorProduct).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.VendorProductID)
                .HasConstraintName("FK_OrderItems_VendorProducts");

            entity.HasOne(d => d.Website).WithMany(p => p.OrderItemWebsites)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Websites");
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasIndex(e => e.CreatedByMemberID, "IX_OrderStatusHistories_CreatedByMemberID");

            entity.HasIndex(e => e.OrderID, "IX_OrderStatusHistories_OrderID");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(500);

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_OrderStatusHistories_Members");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.OrderID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderStatusHistories_Orders");
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasIndex(e => e.CreatedByMemberID, "IX_Pages_CreatedByMemberID");

            entity.HasIndex(e => e.ParentPageID, "IX_Pages_ParentPageID");

            entity.HasIndex(e => e.WebsiteID, "IX_Pages_WebsiteID");

            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.Template).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.Pages)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_Pages_Members");

            entity.HasOne(d => d.ParentPage).WithMany(p => p.InverseParentPage)
                .HasForeignKey(d => d.ParentPageID)
                .HasConstraintName("FK_Pages_Pages");

            entity.HasOne(d => d.Website).WithMany(p => p.Pages)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pages_Websites");
        });

        modelBuilder.Entity<PageTranslation>(entity =>
        {
            entity.HasIndex(e => e.PageID, "IX_PageTranslations_PageID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.Title).HasMaxLength(300);

            entity.HasOne(d => d.Page).WithMany(p => p.PageTranslations)
                .HasForeignKey(d => d.PageID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PageTranslations_Pages");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.BankAccountID, "IX_Payments_BankAccountID");

            entity.HasIndex(e => e.ClientWalletTransactionID, "IX_Payments_ClientWalletTransactionID");

            entity.HasIndex(e => e.CurrencyCode, "IX_Payments_CurrencyCode");

            entity.HasIndex(e => e.OrderID, "IX_Payments_OrderID");

            entity.HasIndex(e => e.PaymentGatewayID, "IX_Payments_PaymentGatewayID");

            entity.HasIndex(e => e.ReceiptFileID, "IX_Payments_ReceiptFileID");

            entity.HasIndex(e => e.VerifiedByMemberID, "IX_Payments_VerifiedByMemberID");

            entity.HasIndex(e => e.WebsiteClientID, "IX_Payments_WebsiteClientID");

            entity.HasIndex(e => e.WebsiteID, "IX_Payments_WebsiteID");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.AmountUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ExchangeRateToUsd).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.GatewayRefNumber).HasMaxLength(100);
            entity.Property(e => e.PaidAt).HasPrecision(0);
            entity.Property(e => e.TrackingCode).HasMaxLength(100);

            entity.HasOne(d => d.BankAccount).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BankAccountID)
                .HasConstraintName("FK_Payments_BankAccounts");

            entity.HasOne(d => d.ClientWalletTransaction).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ClientWalletTransactionID)
                .HasConstraintName("FK_Payments_ClientWalletTransactions");

            entity.HasOne(d => d.CurrencyCodeNavigation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.CurrencyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Currencies");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderID)
                .HasConstraintName("FK_Payments_Orders");

            entity.HasOne(d => d.PaymentGateway).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentGatewayID)
                .HasConstraintName("FK_Payments_PaymentGateways");

            entity.HasOne(d => d.ReceiptFile).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ReceiptFileID)
                .HasConstraintName("FK_Payments_FileRecords");

            entity.HasOne(d => d.VerifiedByMember).WithMany(p => p.Payments)
                .HasForeignKey(d => d.VerifiedByMemberID)
                .HasConstraintName("FK_Payments_Members");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.Payments)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_WebsiteClients");

            entity.HasOne(d => d.Website).WithMany(p => p.Payments)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Websites");
        });

        modelBuilder.Entity<PaymentGateway>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_PaymentGateways_WebsiteID");

            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.ApiSecret).HasMaxLength(500);
            entity.Property(e => e.MerchantID).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Provider).HasMaxLength(50);

            entity.HasOne(d => d.Website).WithMany(p => p.PaymentGateways)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentGateways_Websites");
        });

        modelBuilder.Entity<PaymentRefund>(entity =>
        {
            entity.HasIndex(e => e.BankAccountID, "IX_PaymentRefunds_BankAccountID");

            entity.HasIndex(e => e.ClientWalletTransactionID, "IX_PaymentRefunds_ClientWalletTransactionID");

            entity.HasIndex(e => e.CreatedByMemberID, "IX_PaymentRefunds_CreatedByMemberID");

            entity.HasIndex(e => e.PaymentID, "IX_PaymentRefunds_PaymentID");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RefundedAt).HasColumnType("datetime");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.PaymentRefunds)
                .HasForeignKey(d => d.BankAccountID)
                .HasConstraintName("FK_PaymentRefunds_BankAccounts");

            entity.HasOne(d => d.ClientWalletTransaction).WithMany(p => p.PaymentRefunds)
                .HasForeignKey(d => d.ClientWalletTransactionID)
                .HasConstraintName("FK_PaymentRefunds_ClientWalletTransactions");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.PaymentRefunds)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_PaymentRefunds_Members");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentRefunds)
                .HasForeignKey(d => d.PaymentID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentRefunds_Payments");
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_Policies_WebsiteID");

            entity.Property(e => e.Title)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.HasOne(d => d.Website).WithMany(p => p.Policies)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Policies_Websites");
        });

        modelBuilder.Entity<PolicyRole>(entity =>
        {
            entity.HasIndex(e => e.PolicyID, "IX_PolicyRoles_PolicyID");

            entity.HasIndex(e => e.RoleID, "IX_PolicyRoles_RoleID");

            entity.HasOne(d => d.Policy).WithMany(p => p.PolicyRoles)
                .HasForeignKey(d => d.PolicyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PolicyRoles_Policies");

            entity.HasOne(d => d.Role).WithMany(p => p.PolicyRoles)
                .HasForeignKey(d => d.RoleID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PolicyRoles_Roles");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasIndex(e => e.AuthorMemberID, "IX_Posts_AuthorMemberID");

            entity.HasIndex(e => e.FeaturedImageFileID, "IX_Posts_FeaturedImageFileID");

            entity.HasIndex(e => e.PostTypeID, "IX_Posts_PostTypeID");

            entity.HasIndex(e => e.WebsiteID, "IX_Posts_WebsiteID");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Excerpt).HasMaxLength(1000);
            entity.Property(e => e.PublishedAt).HasColumnType("datetime");
            entity.Property(e => e.ScheduledAt).HasColumnType("datetime");
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.AuthorMember).WithMany(p => p.Posts)
                .HasForeignKey(d => d.AuthorMemberID)
                .HasConstraintName("FK_Posts_Members");

            entity.HasOne(d => d.FeaturedImageFile).WithMany(p => p.Posts)
                .HasForeignKey(d => d.FeaturedImageFileID)
                .HasConstraintName("FK_Posts_FileRecords");

            entity.HasOne(d => d.PostType).WithMany(p => p.Posts)
                .HasForeignKey(d => d.PostTypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Posts_PostTypes");

            entity.HasOne(d => d.Website).WithMany(p => p.Posts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Posts_Websites");

            entity.HasMany(d => d.Tags).WithMany(p => p.Posts)
                .UsingEntity<Dictionary<string, object>>(
                    "PostTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PostTags_Tags"),
                    l => l.HasOne<Post>().WithMany()
                        .HasForeignKey("PostID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PostTags_Posts"),
                    j =>
                    {
                        j.HasKey("PostID", "TagID");
                        j.ToTable("PostTags");
                        j.HasIndex(new[] { "TagID" }, "IX_PostTags_TagID");
                    });
        });

        modelBuilder.Entity<PostCategory>(entity =>
        {
            entity.HasKey(e => new { e.PostID, e.CategoryID });

            entity.HasIndex(e => e.CategoryID, "IX_PostCategories_CategoryID");

            entity.HasOne(d => d.Category).WithMany(p => p.PostCategories)
                .HasForeignKey(d => d.CategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostCategories_Categories");

            entity.HasOne(d => d.Post).WithMany(p => p.PostCategories)
                .HasForeignKey(d => d.PostID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostCategories_Posts");
        });

        modelBuilder.Entity<PostTranslation>(entity =>
        {
            entity.HasIndex(e => e.PostID, "IX_PostTranslations_PostID");

            entity.Property(e => e.Excerpt).HasMaxLength(1000);
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.Title).HasMaxLength(300);

            entity.HasOne(d => d.Post).WithMany(p => p.PostTranslations)
                .HasForeignKey(d => d.PostID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostTranslations_Posts");
        });

        modelBuilder.Entity<PostType>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_PostTypes_WebsiteID");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(100);

            entity.HasOne(d => d.Website).WithMany(p => p.PostTypes)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostTypes_Websites");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.BrandID, "IX_Products_BrandID");

            entity.HasIndex(e => e.CreatedByMemberID, "IX_Products_CreatedByMemberId");

            entity.HasIndex(e => e.FeaturedImageFileID, "IX_Products_FeaturedImageFileID");

            entity.HasIndex(e => e.WebsiteID, "IX_Products_WebsiteID");

            entity.Property(e => e.AvgRating).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ShortDescription).HasMaxLength(1000);
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandID)
                .HasConstraintName("FK_Products_Brands");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.Products)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_Products_Members");

            entity.HasOne(d => d.FeaturedImageFile).WithMany(p => p.Products)
                .HasForeignKey(d => d.FeaturedImageFileID)
                .HasConstraintName("FK_Products_FileRecords");

            entity.HasOne(d => d.Website).WithMany(p => p.Products)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Websites");
        });

        modelBuilder.Entity<ProductAnswer>(entity =>
        {
            entity.HasIndex(e => e.ProductQuestionID, "IX_ProductAnswers_ProductQuestionID");

            entity.HasIndex(e => e.VendorID, "IX_ProductAnswers_VendorID");

            entity.HasIndex(e => e.WebsiteClientID, "IX_ProductAnswers_WebsiteClientID");

            entity.Property(e => e.Body).HasMaxLength(4000);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.ProductQuestion).WithMany(p => p.ProductAnswers)
                .HasForeignKey(d => d.ProductQuestionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductAnswers_ProductQuestions");

            entity.HasOne(d => d.Vendor).WithMany(p => p.ProductAnswers)
                .HasForeignKey(d => d.VendorID)
                .HasConstraintName("FK_ProductAnswers_Vendors");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.ProductAnswers)
                .HasForeignKey(d => d.WebsiteClientID)
                .HasConstraintName("FK_ProductAnswers_WebsiteClients");
        });

        modelBuilder.Entity<ProductAttributeValue>(entity =>
        {
            entity.HasIndex(e => e.AttributeDefinitionID, "IX_ProductAttributeValues_AttributeDefinitionID");

            entity.HasIndex(e => e.AttributeOptionID, "IX_ProductAttributeValues_AttributeOptionID");

            entity.HasIndex(e => e.ProductID, "IX_ProductAttributeValues_ProductID");

            entity.Property(e => e.CustomValue).HasMaxLength(1000);
            entity.Property(e => e.NumericValue).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.AttributeDefinition).WithMany(p => p.ProductAttributeValues)
                .HasForeignKey(d => d.AttributeDefinitionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductAttributeValues_AttributeDefinitions");

            entity.HasOne(d => d.AttributeOption).WithMany(p => p.ProductAttributeValues)
                .HasForeignKey(d => d.AttributeOptionID)
                .HasConstraintName("FK_ProductAttributeValues_AttributeOptions");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAttributeValues)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductAttributeValues_Products");
        });

        modelBuilder.Entity<ProductAttributeValueTranslation>(entity =>
        {
            entity.HasIndex(e => e.ProductAttributeValueID, "IX_ProductAttributeValueTranslations_ProductAttributeValueID");

            entity.Property(e => e.CustomValue).HasMaxLength(1000);
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.ProductAttributeValue).WithMany(p => p.ProductAttributeValueTranslations)
                .HasForeignKey(d => d.ProductAttributeValueID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductAttributeValueTranslations_ProductAttributeValues");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasIndex(e => e.ImageFileID, "IX_ProductCategories_ImageFileID");

            entity.HasIndex(e => e.ParentCategoryID, "IX_ProductCategories_ParentCategoryID");

            entity.HasIndex(e => e.WebsiteID, "IX_ProductCategories_WebsiteID");

            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);

            entity.HasOne(d => d.ImageFile).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.ImageFileID)
                .HasConstraintName("FK_ProductCategories_FileRecords");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryID)
                .HasConstraintName("FK_ProductCategories_ProductCategories");

            entity.HasOne(d => d.Website).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductCategories_Websites");
        });

        modelBuilder.Entity<ProductCategoryMap>(entity =>
        {
            entity.HasKey(e => new { e.ProductID, e.ProductCategoryID });

            entity.HasIndex(e => e.ProductCategoryID, "IX_ProductCategoryMaps_ProductCategoryID");

            entity.HasOne(d => d.ProductCategory).WithMany(p => p.ProductCategoryMaps)
                .HasForeignKey(d => d.ProductCategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductCategoryMaps_ProductCategories");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductCategoryMaps)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductCategoryMaps_Products");
        });

        modelBuilder.Entity<ProductCategoryRelation>(entity =>
        {
            entity.HasKey(e => new { e.ProductID, e.RelatedProductCategoryID, e.RelationType });

            entity.HasIndex(e => e.RelatedProductCategoryID, "IX_ProductCategoryRelations_RelatedProductCategoryID");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductCategoryRelations)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductCategoryRelations_Products");

            entity.HasOne(d => d.RelatedProductCategory).WithMany(p => p.ProductCategoryRelations)
                .HasForeignKey(d => d.RelatedProductCategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductCategoryRelations_ProductCategories");
        });

        modelBuilder.Entity<ProductCategoryTranslation>(entity =>
        {
            entity.HasIndex(e => e.ProductCategoryID, "IX_ProductCategoryTranslations_ProductCategoryID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);

            entity.HasOne(d => d.ProductCategory).WithMany(p => p.ProductCategoryTranslations)
                .HasForeignKey(d => d.ProductCategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductCategoryTranslations_ProductCategories");
        });

        modelBuilder.Entity<ProductMedium>(entity =>
        {
            entity.HasKey(e => new { e.ProductID, e.MediaSetID });

            entity.HasIndex(e => e.MediaSetID, "IX_ProductMedia_MediaSetID");

            entity.HasOne(d => d.MediaSet).WithMany(p => p.ProductMedia)
                .HasForeignKey(d => d.MediaSetID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductMedia_MediaSets");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductMedia)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductMedia_Products");
        });

        modelBuilder.Entity<ProductQuestion>(entity =>
        {
            entity.HasIndex(e => e.ProductID, "IX_ProductQuestions_ProductID");

            entity.HasIndex(e => e.WebsiteClientID, "IX_ProductQuestions_WebsiteClientID");

            entity.HasIndex(e => e.WebsiteID, "IX_ProductQuestions_WebsiteID");

            entity.Property(e => e.Body).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductQuestions)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductQuestions_Products");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.ProductQuestions)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductQuestions_WebsiteClients");

            entity.HasOne(d => d.Website).WithMany(p => p.ProductQuestions)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductQuestions_Websites");
        });

        modelBuilder.Entity<ProductRelation>(entity =>
        {
            entity.HasKey(e => new { e.ProductID, e.RelatedProductID, e.RelationType });

            entity.HasIndex(e => e.RelatedProductID, "IX_ProductRelations_RelatedProductID");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductRelationProducts)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductRelations_Products");

            entity.HasOne(d => d.RelatedProduct).WithMany(p => p.ProductRelationRelatedProducts)
                .HasForeignKey(d => d.RelatedProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductRelations_Products1");
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasIndex(e => e.ProductID, "IX_ProductReviews_ProductID");

            entity.HasIndex(e => e.ProductVariantID, "IX_ProductReviews_ProductVariantID");

            entity.HasIndex(e => e.WebsiteClientID, "IX_ProductReviews_WebsiteClientID");

            entity.HasIndex(e => e.WebsiteID, "IX_ProductReviews_WebsiteID");

            entity.Property(e => e.Body).HasMaxLength(4000);
            entity.Property(e => e.ConsJson).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.ProsJson).HasMaxLength(2000);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductReviews_Products");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductVariantID)
                .HasConstraintName("FK_ProductReviews_ProductVariants");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductReviews_WebsiteClients");

            entity.HasOne(d => d.Website).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductReviews_Websites");
        });

        modelBuilder.Entity<ProductTranslation>(entity =>
        {
            entity.HasIndex(e => e.ProductID, "IX_ProductTranslations_ProductID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ShortDescription).HasMaxLength(1000);
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.Title).HasMaxLength(300);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductTranslations)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductTranslations_Products");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(e => e.ImageFileID, "IX_ProductVariants_ImageFileID");

            entity.HasIndex(e => e.ProductID, "IX_ProductVariants_ProductID");

            entity.HasIndex(e => e.WebsiteID, "IX_ProductVariants_WebsiteID");

            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.CompareAtPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CompareAtPriceUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.OverridePrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ReferencePrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ReferencePriceUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Sku).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Weight).HasColumnType("decimal(10, 3)");

            entity.HasOne(d => d.ImageFile).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ImageFileID)
                .HasConstraintName("FK_ProductVariants_FileRecords");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductVariants_Products");

            entity.HasOne(d => d.Website).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductVariants_Websites");
        });

        modelBuilder.Entity<ProductVariantPriceHistory>(entity =>
        {
            entity.HasIndex(e => new { e.ProductVariantID, e.RecordedAt }, "IX_ProductVariantPriceHistories_ProductVariantID_RecordedAt").IsDescending(false, true);

            entity.Property(e => e.CompareAtPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CompareAtPriceUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.RecordedAt).HasColumnType("datetime");
            entity.Property(e => e.ReferencePrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ReferencePriceUsd).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.ChangedByMember).WithMany(p => p.ProductVariantPriceHistories)
                .HasForeignKey(d => d.ChangedByMemberId)
                .HasConstraintName("FK_ProductVariantPriceHistories_Members");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.ProductVariantPriceHistories)
                .HasForeignKey(d => d.ProductVariantID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductVariantPriceHistories_ProductVariants");
        });

        modelBuilder.Entity<ProductWarning>(entity =>
        {
            entity.HasIndex(e => e.ProductID, "IX_ProductWarnings_ProductID");

            entity.Property(e => e.Severity).HasMaxLength(20);
            entity.Property(e => e.Text).HasMaxLength(1000);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductWarnings)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductWarnings_Products");
        });

        modelBuilder.Entity<ProductWarningTranslation>(entity =>
        {
            entity.HasIndex(e => e.ProductWarningID, "IX_ProductWarningTranslations_ProductWarningID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Text).HasMaxLength(1000);

            entity.HasOne(d => d.ProductWarning).WithMany(p => p.ProductWarningTranslations)
                .HasForeignKey(d => d.ProductWarningID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductWarningTranslations_ProductWarnings");
        });

        modelBuilder.Entity<ProductWarranty>(entity =>
        {
            entity.HasIndex(e => e.ProductID, "IX_ProductWarranties_ProductID");

            entity.HasIndex(e => e.WarrantyID, "IX_ProductWarranties_WarrantyID");

            entity.Property(e => e.CustomDescription).HasMaxLength(2000);
            entity.Property(e => e.CustomTitle).HasMaxLength(200);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductWarranties)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductWarranties_Products");

            entity.HasOne(d => d.Warranty).WithMany(p => p.ProductWarranties)
                .HasForeignKey(d => d.WarrantyID)
                .HasConstraintName("FK_ProductWarranties_Warranties");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Description)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.RoleKey)
                .HasMaxLength(42)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.HasIndex(e => e.ApprovedByMemberID, "IX_Settlements_ApprovedByMemberID");

            entity.HasIndex(e => e.BankAccountID, "IX_Settlements_BankAccountID");

            entity.HasIndex(e => e.CreatedByMemberID, "IX_Settlements_CreatedByMemberID");

            entity.HasIndex(e => e.CurrencyCode, "IX_Settlements_CurrencyCode");

            entity.HasIndex(e => e.SupplierID, "IX_Settlements_SupplierID");

            entity.HasIndex(e => e.TargetWebsiteID, "IX_Settlements_TargetWebsiteID");

            entity.HasIndex(e => e.VendorID, "IX_Settlements_VendorID");

            entity.HasIndex(e => e.WebsiteID, "IX_Settlements_WebsiteID");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentRefNumber).HasMaxLength(100);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.ApprovedByMember).WithMany(p => p.SettlementApprovedByMembers)
                .HasForeignKey(d => d.ApprovedByMemberID)
                .HasConstraintName("FK_Settlements_Member1");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.BankAccountID)
                .HasConstraintName("FK_Settlements_BankAccounts");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.SettlementCreatedByMembers)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_Settlements_Members");

            entity.HasOne(d => d.CurrencyCodeNavigation).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.CurrencyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Settlements_Currencies");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.SupplierID)
                .HasConstraintName("FK_Settlements_Suppliers");

            entity.HasOne(d => d.TargetWebsite).WithMany(p => p.SettlementTargetWebsites)
                .HasForeignKey(d => d.TargetWebsiteID)
                .HasConstraintName("FK_Settlements_Website1");

            entity.HasOne(d => d.Vendor).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.VendorID)
                .HasConstraintName("FK_Settlements_Vendors");

            entity.HasOne(d => d.Website).WithMany(p => p.SettlementWebsites)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Settlements_Websites");
        });

        modelBuilder.Entity<SettlementItem>(entity =>
        {
            entity.HasIndex(e => e.OrderItemID, "IX_SettlementItems_OrderItemID");

            entity.HasIndex(e => e.PaymentID, "IX_SettlementItems_PaymentID");

            entity.HasIndex(e => e.SettlementID, "IX_SettlementItems_SettlementID");

            entity.HasIndex(e => e.StockMovementID, "IX_SettlementItems_StockMovementID");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Description).HasMaxLength(300);

            entity.HasOne(d => d.OrderItem).WithMany(p => p.SettlementItems)
                .HasForeignKey(d => d.OrderItemID)
                .HasConstraintName("FK_SettlementItems_OrderItems");

            entity.HasOne(d => d.Payment).WithMany(p => p.SettlementItems)
                .HasForeignKey(d => d.PaymentID)
                .HasConstraintName("FK_SettlementItems_Payments");

            entity.HasOne(d => d.Settlement).WithMany(p => p.SettlementItems)
                .HasForeignKey(d => d.SettlementID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SettlementItems_Settlements");

            entity.HasOne(d => d.StockMovement).WithMany(p => p.SettlementItems)
                .HasForeignKey(d => d.StockMovementID)
                .HasConstraintName("FK_SettlementItems_StockMovements");
        });

        modelBuilder.Entity<ShippingMethod>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_ShippingMethods_WebsiteID");

            entity.Property(e => e.CarrierName).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.Website).WithMany(p => p.ShippingMethods)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShippingMethods_Websites");
        });

        modelBuilder.Entity<ShippingRate>(entity =>
        {
            entity.HasIndex(e => e.CityID, "IX_ShippingRates_CityID");

            entity.HasIndex(e => e.CountryID, "IX_ShippingRates_CountryID");

            entity.HasIndex(e => e.ShippingMethodID, "IX_ShippingRates_ShippingMethodID");

            entity.HasIndex(e => e.StateID, "IX_ShippingRates_StateID");

            entity.Property(e => e.MaxWeightKg).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.MinWeightKg).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.PriceUsd).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.City).WithMany(p => p.ShippingRates)
                .HasForeignKey(d => d.CityID)
                .HasConstraintName("FK_ShippingRates_Cities");

            entity.HasOne(d => d.Country).WithMany(p => p.ShippingRates)
                .HasForeignKey(d => d.CountryID)
                .HasConstraintName("FK_ShippingRates_Countries");

            entity.HasOne(d => d.ShippingMethod).WithMany(p => p.ShippingRates)
                .HasForeignKey(d => d.ShippingMethodID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShippingRates_ShippingMethods");

            entity.HasOne(d => d.State).WithMany(p => p.ShippingRates)
                .HasForeignKey(d => d.StateID)
                .HasConstraintName("FK_ShippingRates_States");
        });

        modelBuilder.Entity<Slideshow>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_Slideshows_WebsiteID");

            entity.Property(e => e.AspectRatio).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PlacementKey).HasMaxLength(100);

            entity.HasOne(d => d.Website).WithMany(p => p.Slideshows)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Slideshows_Websites");
        });

        modelBuilder.Entity<SlideshowSlide>(entity =>
        {
            entity.HasIndex(e => e.FileID, "IX_SlideshowSlides_FileID");

            entity.HasIndex(e => e.MobileFileID, "IX_SlideshowSlides_MobileFileID");

            entity.HasIndex(e => e.SlideshowID, "IX_SlideshowSlides_SlideshowID");

            entity.Property(e => e.ButtonText).HasMaxLength(100);
            entity.Property(e => e.Caption).HasMaxLength(500);
            entity.Property(e => e.LinkUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.File).WithMany(p => p.SlideshowSlideFiles)
                .HasForeignKey(d => d.FileID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SlideshowSlides_FileRecords");

            entity.HasOne(d => d.MobileFile).WithMany(p => p.SlideshowSlideMobileFiles)
                .HasForeignKey(d => d.MobileFileID)
                .HasConstraintName("FK_SlideshowSlides_FileRecord1");

            entity.HasOne(d => d.Slideshow).WithMany(p => p.SlideshowSlides)
                .HasForeignKey(d => d.SlideshowID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SlideshowSlides_Slideshows");
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasIndex(e => e.CountryID, "IX_States_CountryID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(48);

            entity.HasOne(d => d.Country).WithMany(p => p.States)
                .HasForeignKey(d => d.CountryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_States_Countries");
        });

        modelBuilder.Entity<StateTranslation>(entity =>
        {
            entity.HasIndex(e => e.StateID, "IX_StateTranslations_StateID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(48);

            entity.HasOne(d => d.State).WithMany(p => p.StateTranslations)
                .HasForeignKey(d => d.StateID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StateTranslations_States");
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasIndex(e => e.CreatedByMemberID, "IX_StockMovements_CreatedByMemberID");

            entity.HasIndex(e => e.CurrencyCode, "IX_StockMovements_CurrencyCode");

            entity.HasIndex(e => e.OrderID, "IX_StockMovements_OrderID");

            entity.HasIndex(e => e.OrderItemID, "IX_StockMovements_OrderItemID");

            entity.HasIndex(e => e.ProductVariantID, "IX_StockMovements_ProductVariantID");

            entity.HasIndex(e => e.SupplierID, "IX_StockMovements_SupplierID");

            entity.HasIndex(e => e.WebsiteID, "IX_StockMovements_WebsiteID");

            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ExchangeRateToUsd)
                .HasComment("website's local currency rate snapshotted at the moment of this stock entry/exit, so accounting and reports can be reconstructed in local currency at that point in time")
                .HasColumnType("decimal(18, 6)");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.UnitCostUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.UnitSalePrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.UnitSalePriceUsd).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_StockMovements_Members");

            entity.HasOne(d => d.CurrencyCodeNavigation).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.CurrencyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockMovements_Currencies");

            entity.HasOne(d => d.Order).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.OrderID)
                .HasConstraintName("FK_StockMovements_Orders");

            entity.HasOne(d => d.OrderItem).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.OrderItemID)
                .HasConstraintName("FK_StockMovements_OrderItems");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.ProductVariantID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockMovements_ProductVariants");

            entity.HasOne(d => d.Supplier).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.SupplierID)
                .HasConstraintName("FK_StockMovements_Suppliers");

            entity.HasOne(d => d.Website).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockMovements_Websites");
        });

        modelBuilder.Entity<SupportInteraction>(entity =>
        {
            entity.HasIndex(e => e.CreatedByMemberID, "IX_SupportInteractions_CreatedByMemberID");
            entity.HasIndex(e => e.CreatedAt, "IX_SupportInteractions_CreatedAt");
            entity.HasIndex(e => e.RelatedOrderID, "IX_SupportInteractions_RelatedOrderID");
            entity.HasIndex(e => e.SupportSessionID, "IX_SupportInteractions_SupportSessionID");

            entity.Property(e => e.Body).HasMaxLength(4000);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.SupportInteractions)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_SupportInteractions_Members");

            entity.HasOne(d => d.RelatedOrder).WithMany(p => p.SupportInteractions)
                .HasForeignKey(d => d.RelatedOrderID)
                .HasConstraintName("FK_SupportInteractions_Orders");

            entity.HasOne(d => d.SupportSession).WithMany(p => p.SupportInteractions)
                .HasForeignKey(d => d.SupportSessionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupportInteractions_SupportSessions");
        });

        modelBuilder.Entity<SupportSession>(entity =>
        {
            entity.HasIndex(e => e.AssignedMemberID, "IX_SupportSessions_AssignedMemberID");
            entity.HasIndex(e => e.CellphoneSnapshot, "IX_SupportSessions_CellphoneSnapshot");
            entity.HasIndex(e => e.CreatedAt, "IX_SupportSessions_CreatedAt");
            entity.HasIndex(e => e.CreatedByMemberID, "IX_SupportSessions_CreatedByMemberID");
            entity.HasIndex(e => e.RelatedOrderID, "IX_SupportSessions_RelatedOrderID");
            entity.HasIndex(e => e.Status, "IX_SupportSessions_Status");
            entity.HasIndex(e => e.WebsiteClientID, "IX_SupportSessions_WebsiteClientID");
            entity.HasIndex(e => e.WebsiteID, "IX_SupportSessions_WebsiteID");
            entity.HasIndex(e => new { e.WebsiteID, e.SessionNumber }, "IX_SupportSessions_Website_SessionNumber")
                .IsUnique();

            entity.Property(e => e.CallbackAt).HasColumnType("datetime");
            entity.Property(e => e.CallbackNote).HasMaxLength(500);
            entity.Property(e => e.CellphoneSnapshot)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.ClosedAt).HasColumnType("datetime");
            entity.Property(e => e.CountryCodeSnapshot)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CustomerNameSnapshot).HasMaxLength(128);
            entity.Property(e => e.EmailSnapshot)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.FirstResponseAt).HasColumnType("datetime");
            entity.Property(e => e.FirstResponseDueAt).HasColumnType("datetime");
            entity.Property(e => e.LastInteractionAt).HasColumnType("datetime");
            entity.Property(e => e.ResolveDueAt).HasColumnType("datetime");
            entity.Property(e => e.ResolvedAt).HasColumnType("datetime");
            entity.Property(e => e.SessionNumber).HasMaxLength(30);
            entity.Property(e => e.Subject).HasMaxLength(256);
            entity.Property(e => e.Tags).HasMaxLength(256);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.AssignedMember).WithMany(p => p.SupportSessionAssignedMembers)
                .HasForeignKey(d => d.AssignedMemberID)
                .HasConstraintName("FK_SupportSessions_AssignedMembers");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.SupportSessionCreatedByMembers)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_SupportSessions_CreatedByMembers");

            entity.HasOne(d => d.RelatedOrder).WithMany(p => p.SupportSessions)
                .HasForeignKey(d => d.RelatedOrderID)
                .HasConstraintName("FK_SupportSessions_Orders");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.SupportSessions)
                .HasForeignKey(d => d.WebsiteClientID)
                .HasConstraintName("FK_SupportSessions_WebsiteClients");

            entity.HasOne(d => d.Website).WithMany(p => p.SupportSessions)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupportSessions_Websites");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_Suppliers_WebsiteID");

            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.Website).WithMany(p => p.Suppliers)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Suppliers_Websites");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_Tags_WebsiteID");

            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Slug).HasMaxLength(150);

            entity.HasOne(d => d.Website).WithMany(p => p.Tags)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tags_Websites");
        });

        modelBuilder.Entity<TagTranslation>(entity =>
        {
            entity.HasIndex(e => e.TagID, "IX_TagTranslations_TagID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Slug).HasMaxLength(150);

            entity.HasOne(d => d.Tag).WithMany(p => p.TagTranslations)
                .HasForeignKey(d => d.TagID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TagTranslations_Tags");
        });

        modelBuilder.Entity<TaxRate>(entity =>
        {
            entity.HasIndex(e => e.CountryID, "IX_TaxRates_CountryID");

            entity.HasIndex(e => e.StateID, "IX_TaxRates_StateID");

            entity.HasIndex(e => e.WebsiteID, "IX_TaxRates_WebsiteID");

            entity.Property(e => e.Rate).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.Country).WithMany(p => p.TaxRates)
                .HasForeignKey(d => d.CountryID)
                .HasConstraintName("FK_TaxRates_Countries");

            entity.HasOne(d => d.State).WithMany(p => p.TaxRates)
                .HasForeignKey(d => d.StateID)
                .HasConstraintName("FK_TaxRates_States");

            entity.HasOne(d => d.Website).WithMany(p => p.TaxRates)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaxRates_Websites");
        });

        modelBuilder.Entity<VariantAttributeValue>(entity =>
        {
            entity.HasKey(e => new { e.ProductVariantID, e.AttributeDefinitionID });

            entity.HasIndex(e => e.AttributeDefinitionID, "IX_VariantAttributeValues_AttributeDefinitionID");

            entity.HasIndex(e => e.AttributeOptionID, "IX_VariantAttributeValues_AttributeOptionID");

            entity.HasOne(d => d.AttributeDefinition).WithMany(p => p.VariantAttributeValues)
                .HasForeignKey(d => d.AttributeDefinitionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VariantAttributeValues_AttributeDefinitions");

            entity.HasOne(d => d.AttributeOption).WithMany(p => p.VariantAttributeValues)
                .HasForeignKey(d => d.AttributeOptionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VariantAttributeValues_AttributeOptions");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.VariantAttributeValues)
                .HasForeignKey(d => d.ProductVariantID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VariantAttributeValues_ProductVariants");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasIndex(e => e.LinkedWebsiteID, "IX_Vendors_LinkedWebsiteID");

            entity.HasIndex(e => e.LogoFileID, "IX_Vendors_LogoFileID");

            entity.HasIndex(e => e.MemberID, "IX_Vendors_MemberID")
                .IsUnique()
                .HasFilter("([MemberID] IS NOT NULL)");

            entity.HasIndex(e => e.WebsiteID, "IX_Vendors_WebsiteID");

            entity.Property(e => e.AvailableCredit).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.AvailableCreditUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreditDays).HasComment("number of days after the settlement period ends before payment is due; only meaningful when SettlementMode = Credit");
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreditLimitUsd)
                .HasComment("maximum outstanding credit balance allowed for this vendor, in USD; only meaningful when SettlementMode = Credit")
                .HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Rating).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.SettlementMode).HasComment("0 = Immediate: every purchase from this vendor is settled instantly like a normal cash purchase. 1 = Credit: purchases accrue as credit and are batched into a periodic Settlements record due on CreditDays");
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.VendorType).HasComment("0 = Display-only title, 1 = Member login (manage own catalog), 2 = Linked website (inter-site virtual credit)");

            entity.HasOne(d => d.LinkedWebsite).WithMany(p => p.VendorLinkedWebsites)
                .HasForeignKey(d => d.LinkedWebsiteID)
                .HasConstraintName("FK_Vendors_LinkedWebsites");

            entity.HasOne(d => d.LogoFile).WithMany(p => p.Vendors)
                .HasForeignKey(d => d.LogoFileID)
                .HasConstraintName("FK_Vendors_FileRecords");

            entity.HasOne(d => d.Member).WithOne(p => p.Vendor)
                .HasForeignKey<Vendor>(d => d.MemberID)
                .HasConstraintName("FK_Vendors_Members");

            entity.HasOne(d => d.Website).WithMany(p => p.VendorWebsites)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vendors_Websites");
        });

        modelBuilder.Entity<VendorCreditTransaction>(entity =>
        {
            entity.HasIndex(e => e.CreatedByMemberID, "IX_VendorCreditTransactions_CreatedByMemberID");

            entity.HasIndex(e => e.MirrorOrderID, "IX_VendorCreditTransactions_MirrorOrderID");

            entity.HasIndex(e => e.SourceOrderItemID, "IX_VendorCreditTransactions_SourceOrderItemID");

            entity.HasIndex(e => e.VendorID, "IX_VendorCreditTransactions_VendorID");

            entity.HasIndex(e => e.WebsiteID, "IX_VendorCreditTransactions_WebsiteID");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.AmountUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.BalanceAfter).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.BalanceAfterUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.SourceType).HasComment("1 = Grant, 2 = Sale, 3 = Adjustment, 4 = Refund");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.VendorCreditTransactions)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_VendorCreditTransactions_Members");

            entity.HasOne(d => d.MirrorOrder).WithMany(p => p.VendorCreditTransactionMirrorOrders)
                .HasForeignKey(d => d.MirrorOrderID)
                .HasConstraintName("FK_VendorCreditTransactions_Orders");

            entity.HasOne(d => d.SourceOrderItem).WithMany(p => p.VendorCreditTransactions)
                .HasForeignKey(d => d.SourceOrderItemID)
                .HasConstraintName("FK_VendorCreditTransactions_OrderItems");

            entity.HasOne(d => d.Vendor).WithMany(p => p.VendorCreditTransactions)
                .HasForeignKey(d => d.VendorID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VendorCreditTransactions_Vendors");

            entity.HasOne(d => d.Website).WithMany(p => p.VendorCreditTransactions)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VendorCreditTransactions_Websites");
        });

        modelBuilder.Entity<VendorProduct>(entity =>
        {
            entity.HasIndex(e => e.ProductVariantID, "IX_VendorProducts_ProductVariantID");

            entity.HasIndex(e => e.VendorID, "IX_VendorProducts_VendorID");

            entity.HasIndex(e => e.WebsiteID, "IX_VendorProducts_WebsiteID");

            entity.Property(e => e.OverridePrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.OverridePriceLocal).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ReferencePrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ReferencePriceUsd).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.VendorProducts)
                .HasForeignKey(d => d.ProductVariantID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VendorProducts_ProductVariants");

            entity.HasOne(d => d.Vendor).WithMany(p => p.VendorProducts)
                .HasForeignKey(d => d.VendorID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VendorProducts_Vendors");

            entity.HasOne(d => d.Website).WithMany(p => p.VendorProducts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VendorProducts_Websites");
        });

        modelBuilder.Entity<VendorTranslation>(entity =>
        {
            entity.HasIndex(e => e.VendorID, "IX_VendorTranslations_VendorID");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Vendor).WithMany(p => p.VendorTranslations)
                .HasForeignKey(d => d.VendorID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VendorTranslations_Vendors");
        });

        modelBuilder.Entity<Warranty>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_Warranties_WebsiteID");

            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ProviderName).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Website).WithMany(p => p.Warranties)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Warranties_Websites");
        });

        modelBuilder.Entity<WarrantyTranslation>(entity =>
        {
            entity.HasIndex(e => e.WarrantyID, "IX_WarrantyTranslations_WarrantyID");

            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ProviderName).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Warranty).WithMany(p => p.WarrantyTranslations)
                .HasForeignKey(d => d.WarrantyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WarrantyTranslations_Warranties");
        });

        modelBuilder.Entity<Website>(entity =>
        {
            entity.HasIndex(e => e.DefaultCurrencyCode, "IX_Websites_DefaultCurrencyCode");

            entity.HasIndex(e => e.FaveIconFileID, "IX_Websites_FaveIconFileID");

            entity.HasIndex(e => e.LogoFileID, "IX_Websites_LogoFileID");

            entity.Property(e => e.BrandName)
                .HasMaxLength(60)
                .HasComment("show in title of pages");
            entity.Property(e => e.DefaultCurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DefaultLanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Email)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Manager).HasMaxLength(30);
            entity.Property(e => e.Mobile)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.StorePricesInUsd)
                .HasDefaultValue(false)
                .HasComment("When true, also persist USD dual columns; default site currency is always operational authority.");
            entity.Property(e => e.TradeName).HasMaxLength(32);
            entity.Property(e => e.WebsiteAddress)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.HasOne(d => d.DefaultCurrencyCodeNavigation).WithMany(p => p.Websites)
                .HasForeignKey(d => d.DefaultCurrencyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Websites_Currencies");

            entity.HasOne(d => d.FaveIconFile).WithMany(p => p.WebsiteFaveIconFiles)
                .HasForeignKey(d => d.FaveIconFileID)
                .HasConstraintName("FK_Websites_FileRecord1");

            entity.HasOne(d => d.LogoFile).WithMany(p => p.WebsiteLogoFiles)
                .HasForeignKey(d => d.LogoFileID)
                .HasConstraintName("FK_Websites_FileRecords");
        });

        modelBuilder.Entity<WebsiteCaptchaSetting>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "UQ_WebsiteCaptchaSettings_WebsiteID").IsUnique();

            entity.Property(e => e.TurnstileSecretKey).HasMaxLength(200);
            entity.Property(e => e.TurnstileSiteKey).HasMaxLength(200);

            entity.HasOne(d => d.Website).WithOne(p => p.WebsiteCaptchaSetting)
                .HasForeignKey<WebsiteCaptchaSetting>(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteCaptchaSettings_Websites");
        });

        modelBuilder.Entity<WebsiteClient>(entity =>
        {
            entity.HasIndex(e => e.AvatarID, "IX_WebsiteClients_AvatarID");

            entity.HasIndex(e => e.WebsiteID, "IX_WebsiteClients_WebsiteID");

            entity.Property(e => e.Cellphone)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.CountryCode)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Givenname).HasMaxLength(42);
            entity.Property(e => e.Password)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Surname).HasMaxLength(42);

            entity.HasOne(d => d.Avatar).WithMany(p => p.WebsiteClients)
                .HasForeignKey(d => d.AvatarID)
                .HasConstraintName("FK_WebsiteClients_FileRecords");

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteClients)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteClients_Websites");
        });

        modelBuilder.Entity<WebsiteClientAddress>(entity =>
        {
            entity.HasIndex(e => e.CityId, "IX_WebsiteClientAddresses_CityId");

            entity.HasIndex(e => e.CountryId, "IX_WebsiteClientAddresses_CountryId");

            entity.HasIndex(e => e.WebsiteClientID, "IX_WebsiteClientAddresses_WebsiteClientID");

            entity.Property(e => e.AddressLine).HasMaxLength(500);
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.ReceiverName).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.City).WithMany(p => p.WebsiteClientAddresses)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_WebsiteClientAddresses_Cities");

            entity.HasOne(d => d.Country).WithMany(p => p.WebsiteClientAddresses)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("FK_WebsiteClientAddresses_Countries");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.WebsiteClientAddresses)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteClientAddresses_WebsiteClients");
        });

        modelBuilder.Entity<WebsiteClientForgetPassword>(entity =>
        {
            entity.HasIndex(e => e.WebsiteClientID, "IX_WebsiteClientForgetPasswords_WebsiteClientID");

            entity.Property(e => e.ForgetKey)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.LogTime).HasColumnType("datetime");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.WebsiteClientForgetPasswords)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteClientForgetPasswords_WebsiteClients");
        });

        modelBuilder.Entity<WebsiteFeature>(entity =>
        {
            entity.HasIndex(e => new { e.WebsiteID, e.FeatureKey }, "UQ_WebsiteFeatures_WebsiteID_FeatureKey").IsUnique();

            entity.Property(e => e.CreatedAt).HasPrecision(0);

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteFeatures)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteFeatures_Websites");
        });

        modelBuilder.Entity<WebsiteIP>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_WebsiteIPs_WebsiteID");

            entity.Property(e => e.EndIP)
                .HasMaxLength(45)
                .IsUnicode(false);
            entity.Property(e => e.Label).HasMaxLength(30);
            entity.Property(e => e.StartIP)
                .HasMaxLength(45)
                .IsUnicode(false);

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteIPs)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteIPs_Websites");
        });

        modelBuilder.Entity<WebsiteRedirect>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_WebsiteRedirects_WebsiteID");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.SourcePath).HasMaxLength(500);
            entity.Property(e => e.TargetPath).HasMaxLength(500);

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteRedirects)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteRedirects_Websites");
        });

        modelBuilder.Entity<WebsiteScript>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_WebsiteScripts_WebsiteID");

            entity.Property(e => e.LogTime).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RawContent).HasMaxLength(4000);

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteScripts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteScripts_Websites");
        });

        modelBuilder.Entity<WebsiteSeoSetting>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_WebsiteSeoSettings_WebsiteID");

            entity.Property(e => e.DefaultMetaDescription).HasMaxLength(158);
            entity.Property(e => e.DefaultMetaTitle)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasComment("Page | {SiteName}");
            entity.Property(e => e.TitleSeparator)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteSeoSettings)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteSeoSettings_Websites");
        });

        modelBuilder.Entity<WebsiteSocialLink>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_WebsiteSocialLinks_WebsiteID");

            entity.Property(e => e.SocialIcon)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.SocialName).HasMaxLength(64);
            entity.Property(e => e.UrlAddress).HasMaxLength(80);

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteSocialLinks)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteSocialLinks_Websites");
        });

        modelBuilder.Entity<WebsiteStorageSetting>(entity =>
        {
            entity.HasKey(e => e.WebsiteStorageSettingsID);

            entity.HasIndex(e => e.WebsiteID, "IX_WebsiteStorageSettings_WebsiteID");

            entity.Property(e => e.AllowedExtensions)
                .HasMaxLength(710)
                .IsUnicode(false);
            entity.Property(e => e.StorageSettingsJSON).HasMaxLength(2000);

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteStorageSettings)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteStorageSettings_Websites");
        });

        modelBuilder.Entity<WebsiteTheme>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_WebsiteThemes_WebsiteID");

            entity.HasIndex(e => new { e.WebsiteID, e.Slug }, "IX_WebsiteThemes_WebsiteID_Slug").IsUnique();

            entity.Property(e => e.Author).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(64);
            entity.Property(e => e.Version).HasMaxLength(50);

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteThemes)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteThemes_Websites");
        });

        modelBuilder.Entity<WebsiteWatermarkSetting>(entity =>
        {
            entity.HasIndex(e => e.WatermarkFileID, "IX_WebsiteWatermarkSettings_WatermarkFileID");

            entity.HasIndex(e => e.WebsiteID, "IX_WebsiteWatermarkSettings_WebsiteID");

            entity.HasOne(d => d.WatermarkFile).WithMany(p => p.WebsiteWatermarkSettings)
                .HasForeignKey(d => d.WatermarkFileID)
                .HasConstraintName("FK_WebsiteWatermarkSettings_FileRecords");

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteWatermarkSettings)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteWatermarkSettings_Websites");
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_Wishlists_WebsiteID");

            entity.HasIndex(e => e.WebsiteClientID, "UQ_Wishlists_WebsiteClientID").IsUnique();

            entity.Property(e => e.CreatedAt).HasPrecision(0);

            entity.HasOne(d => d.WebsiteClient).WithOne(p => p.Wishlist)
                .HasForeignKey<Wishlist>(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Wishlists_WebsiteClients");

            entity.HasOne(d => d.Website).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Wishlists_Websites");
        });

        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.HasIndex(e => e.ProductVariantID, "IX_WishlistItems_ProductVariantID");

            entity.HasIndex(e => new { e.WishlistID, e.ProductVariantID }, "UQ_WishlistItems_WishlistID_ProductVariantID").IsUnique();

            entity.Property(e => e.AddedAt).HasPrecision(0);

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.WishlistItems)
                .HasForeignKey(d => d.ProductVariantID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WishlistItems_ProductVariants");

            entity.HasOne(d => d.Wishlist).WithMany(p => p.WishlistItems)
                .HasForeignKey(d => d.WishlistID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WishlistItems_Wishlists");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
