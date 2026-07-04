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

    public virtual DbSet<AttributeDefinition> AttributeDefinitions { get; set; }

    public virtual DbSet<AttributeDefinitionTranslation> AttributeDefinitionTranslations { get; set; }

    public virtual DbSet<AttributeOption> AttributeOptions { get; set; }

    public virtual DbSet<AttributeOptionTranslation> AttributeOptionTranslations { get; set; }

    public virtual DbSet<Bank> Banks { get; set; }

    public virtual DbSet<BankAccount> BankAccounts { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<BrandTranslation> BrandTranslations { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryTranslation> CategoryTranslations { get; set; }

    public virtual DbSet<ChartOfAccount> ChartOfAccounts { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<CityTranslation> CityTranslations { get; set; }

    public virtual DbSet<ContactUsMessage> ContactUsMessages { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<CountryTranslation> CountryTranslations { get; set; }

    public virtual DbSet<CurrencyRate> CurrencyRates { get; set; }

    public virtual DbSet<EmailSetting> EmailSettings { get; set; }

    public virtual DbSet<EmailSubscribe> EmailSubscribes { get; set; }

    public virtual DbSet<FileAlbum> FileAlbums { get; set; }

    public virtual DbSet<FileRecord> FileRecords { get; set; }

    public virtual DbSet<FileRecordTag> FileRecordTags { get; set; }

    public virtual DbSet<FileTag> FileTags { get; set; }

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

    public virtual DbSet<ProductContentSection> ProductContentSections { get; set; }

    public virtual DbSet<ProductContentSectionTranslation> ProductContentSectionTranslations { get; set; }

    public virtual DbSet<ProductMedium> ProductMedia { get; set; }

    public virtual DbSet<ProductQuestion> ProductQuestions { get; set; }

    public virtual DbSet<ProductRelation> ProductRelations { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<ProductTranslation> ProductTranslations { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<ProductWarning> ProductWarnings { get; set; }

    public virtual DbSet<ProductWarningTranslation> ProductWarningTranslations { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Settlement> Settlements { get; set; }

    public virtual DbSet<SettlementItem> SettlementItems { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<StateTranslation> StateTranslations { get; set; }

    public virtual DbSet<StockMovement> StockMovements { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TagTranslation> TagTranslations { get; set; }

    public virtual DbSet<VariantAttributeValue> VariantAttributeValues { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }

    public virtual DbSet<VendorProduct> VendorProducts { get; set; }

    public virtual DbSet<VendorTranslation> VendorTranslations { get; set; }

    public virtual DbSet<Website> Websites { get; set; }

    public virtual DbSet<WebsiteClient> WebsiteClients { get; set; }

    public virtual DbSet<WebsiteClientAddress> WebsiteClientAddresses { get; set; }

    public virtual DbSet<WebsiteClientForgetPassword> WebsiteClientForgetPasswords { get; set; }

    public virtual DbSet<WebsiteIP> WebsiteIPs { get; set; }

    public virtual DbSet<WebsiteRedirect> WebsiteRedirects { get; set; }

    public virtual DbSet<WebsiteScript> WebsiteScripts { get; set; }

    public virtual DbSet<WebsiteSeoSetting> WebsiteSeoSettings { get; set; }

    public virtual DbSet<WebsiteSocialLink> WebsiteSocialLinks { get; set; }

    public virtual DbSet<WebstieStorageSetting> WebstieStorageSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttributeDefinition>(entity =>
        {
            entity.ToTable("AttributeDefinition");

            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Unit).HasMaxLength(30);

            entity.HasOne(d => d.Website).WithMany(p => p.AttributeDefinitions)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttributeDefinition_Website");
        });

        modelBuilder.Entity<AttributeDefinitionTranslation>(entity =>
        {
            entity.ToTable("AttributeDefinitionTranslation");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Unit).HasMaxLength(30);

            entity.HasOne(d => d.AttributeDefinition).WithMany(p => p.AttributeDefinitionTranslations)
                .HasForeignKey(d => d.AttributeDefinitionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttributeDefinitionTranslation_AttributeDefinition");
        });

        modelBuilder.Entity<AttributeOption>(entity =>
        {
            entity.ToTable("AttributeOption");

            entity.Property(e => e.ColorHex)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Value).HasMaxLength(2000);

            entity.HasOne(d => d.AttributeDefinition).WithMany(p => p.AttributeOptions)
                .HasForeignKey(d => d.AttributeDefinitionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttributeOption_AttributeDefinition");
        });

        modelBuilder.Entity<AttributeOptionTranslation>(entity =>
        {
            entity.ToTable("AttributeOptionTranslation");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Value).HasMaxLength(2000);

            entity.HasOne(d => d.AttributeOption).WithMany(p => p.AttributeOptionTranslations)
                .HasForeignKey(d => d.AttributeOptionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttributeOptionTranslation_AttributeOption");
        });

        modelBuilder.Entity<Bank>(entity =>
        {
            entity.ToTable("Bank");

            entity.Property(e => e.BankCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(64);

            entity.HasOne(d => d.LogoFile).WithMany(p => p.Banks)
                .HasForeignKey(d => d.LogoFileID)
                .HasConstraintName("FK_Bank_FileRecord");

            entity.HasOne(d => d.Website).WithMany(p => p.Banks)
                .HasForeignKey(d => d.WebsiteID)
                .HasConstraintName("FK_Bank_Website");
        });

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CardNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.IBAN)
                .HasMaxLength(34)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_BankAccounts_IsActive");
            entity.Property(e => e.IsForOfflinePayment).HasDefaultValue(true, "DF_BankAccounts_IsForOfflinePayment");
            entity.Property(e => e.OwnerName).HasMaxLength(90);
            entity.Property(e => e.Title).HasMaxLength(70);

            entity.HasOne(d => d.Bank).WithMany(p => p.BankAccounts)
                .HasForeignKey(d => d.BankID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BankAccounts_Bank");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.BankAccounts)
                .HasForeignKey(d => d.CreatedByMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BankAccounts_Member");

            entity.HasOne(d => d.Website).WithMany(p => p.BankAccounts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BankAccounts_Website");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Brands_IsActive_1");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);

            entity.HasOne(d => d.LogoFile).WithMany(p => p.Brands)
                .HasForeignKey(d => d.LogoFileID)
                .HasConstraintName("FK_Brands_FileRecord");

            entity.HasOne(d => d.Website).WithMany(p => p.Brands)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Brands_Website");
        });

        modelBuilder.Entity<BrandTranslation>(entity =>
        {
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

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Category_IsActive");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryID)
                .HasConstraintName("FK_Category_Category");

            entity.HasOne(d => d.PostType).WithMany(p => p.Categories)
                .HasForeignKey(d => d.PostTypeID)
                .HasConstraintName("FK_Category_PostType");

            entity.HasOne(d => d.Website).WithMany(p => p.Categories)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Category_Website");
        });

        modelBuilder.Entity<CategoryTranslation>(entity =>
        {
            entity.ToTable("CategoryTranslation");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryTranslations)
                .HasForeignKey(d => d.CategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoryTranslation_Category");
        });

        modelBuilder.Entity<ChartOfAccount>(entity =>
        {
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_ChartOfAccounts_IsActive");
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.ParentAccount).WithMany(p => p.InverseParentAccount)
                .HasForeignKey(d => d.ParentAccountID)
                .HasConstraintName("FK_ChartOfAccounts_ChartOfAccounts");

            entity.HasOne(d => d.Website).WithMany(p => p.ChartOfAccounts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChartOfAccounts_Website");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("City");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(48);

            entity.HasOne(d => d.Country).WithMany(p => p.Cities)
                .HasForeignKey(d => d.CountryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_City_Country");

            entity.HasOne(d => d.State).WithMany(p => p.Cities)
                .HasForeignKey(d => d.StateID)
                .HasConstraintName("FK_City_State");
        });

        modelBuilder.Entity<CityTranslation>(entity =>
        {
            entity.ToTable("CityTranslation");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(48);

            entity.HasOne(d => d.City).WithMany(p => p.CityTranslations)
                .HasForeignKey(d => d.CityID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_City_Translation_City");
        });

        modelBuilder.Entity<ContactUsMessage>(entity =>
        {
            entity.HasKey(e => e.ContactUsMessagesID);

            entity.ToTable("ContactUsMessage");

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
                .HasConstraintName("FK_ContactUsMessage_Website");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Country");

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
            entity.ToTable("CountryTranslation");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(42);

            entity.HasOne(d => d.Country).WithMany(p => p.CountryTranslations)
                .HasForeignKey(d => d.CountryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CountryTranslation_Country");
        });

        modelBuilder.Entity<CurrencyRate>(entity =>
        {
            entity.ToTable("CurrencyRate");

            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LastUpdate).HasColumnType("datetime");
            entity.Property(e => e.USDToCurrency).HasColumnType("decimal(18, 6)");

            entity.HasOne(d => d.Website).WithMany(p => p.CurrencyRates)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CurrencyRate_Website");
        });

        modelBuilder.Entity<EmailSetting>(entity =>
        {
            entity.ToTable("EmailSetting");

            entity.Property(e => e.EmailAddress)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.MailName).HasMaxLength(64);
            entity.Property(e => e.MailServer)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Password).HasMaxLength(256);

            entity.HasOne(d => d.Website).WithMany(p => p.EmailSettings)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmailSetting_Website");
        });

        modelBuilder.Entity<EmailSubscribe>(entity =>
        {
            entity.ToTable("EmailSubscribe");

            entity.Property(e => e.Email)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.LogTime).HasColumnType("datetime");

            entity.HasOne(d => d.Member).WithMany(p => p.EmailSubscribes)
                .HasForeignKey(d => d.MemberID)
                .HasConstraintName("FK_EmailSubscribe_Member");
        });

        modelBuilder.Entity<FileAlbum>(entity =>
        {
            entity.ToTable("FileAlbum");

            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(400);
            entity.Property(e => e.Name).HasMaxLength(120);

            entity.HasOne(d => d.Website).WithMany(p => p.FileAlbums)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileAlbum_Website");
        });

        modelBuilder.Entity<FileRecord>(entity =>
        {
            entity.ToTable("FileRecord");

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

            entity.HasOne(d => d.FileAlbum).WithMany(p => p.FileRecords)
                .HasForeignKey(d => d.FileAlbumID)
                .HasConstraintName("FK_FileRecord_FileAlbum");

            entity.HasOne(d => d.UploaderMember).WithMany(p => p.FileRecords)
                .HasForeignKey(d => d.UploaderMemberID)
                .HasConstraintName("FK_FileRecord_Member");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.FileRecords)
                .HasForeignKey(d => d.WebsiteClientID)
                .HasConstraintName("FK_FileRecord_WebsiteClient");

            entity.HasOne(d => d.Website).WithMany(p => p.FileRecords)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileRecord_Website");

            entity.HasOne(d => d.WebsiteStorageSettings).WithMany(p => p.FileRecords)
                .HasForeignKey(d => d.WebsiteStorageSettingsID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileRecord_WebstieStorageSettings");
        });

        modelBuilder.Entity<FileRecordTag>(entity =>
        {
            entity.ToTable("FileRecordTag");

            entity.HasOne(d => d.FileRecord).WithMany(p => p.FileRecordTags)
                .HasForeignKey(d => d.FileRecordID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileRecordTag_FileRecord");

            entity.HasOne(d => d.FileTag).WithMany(p => p.FileRecordTags)
                .HasForeignKey(d => d.FileTagID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileRecordTag_FileTag");
        });

        modelBuilder.Entity<FileTag>(entity =>
        {
            entity.ToTable("FileTag");

            entity.Property(e => e.Name).HasMaxLength(60);

            entity.HasOne(d => d.Website).WithMany(p => p.FileTags)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileTag_Website");
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
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
                .HasConstraintName("FK_InventoryItems_Website");
        });

        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.JournalEntrieID);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_JournalEntries_CreatedAt")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EntryNumber).HasMaxLength(30);

            entity.HasOne(d => d.Website).WithMany(p => p.JournalEntries)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalEntries_Website");
        });

        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
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
            entity.HasKey(e => e.LangaugeID);

            entity.ToTable("Language");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LanguageCodeISO)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name)
                .HasMaxLength(32)
                .IsUnicode(false);

            entity.HasOne(d => d.Website).WithMany(p => p.Languages)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Language_Website");
        });

        modelBuilder.Entity<LocalizationKey>(entity =>
        {
            entity.ToTable("LocalizationKey");

            entity.Property(e => e.DefaultValue).HasMaxLength(2000);
            entity.Property(e => e.ItemKey)
                .HasMaxLength(72)
                .IsUnicode(false);

            entity.HasOne(d => d.Website).WithMany(p => p.LocalizationKeys)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LocalizationKey_Language");
        });

        modelBuilder.Entity<LocalizationValue>(entity =>
        {
            entity.ToTable("LocalizationValue");

            entity.Property(e => e.ItemValue).HasMaxLength(2000);
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.LocalizationKey).WithMany(p => p.LocalizationValues)
                .HasForeignKey(d => d.LocalizationKeyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LocalizationValue_LocalizationKey");
        });

        modelBuilder.Entity<LoginTry>(entity =>
        {
            entity.ToTable("LoginTry");

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
                .HasConstraintName("FK_LoginTry_Website");
        });

        modelBuilder.Entity<MediaSet>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_MediaSets_CreatedAt_1")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Website).WithMany(p => p.MediaSets)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MediaSets_Website");
        });

        modelBuilder.Entity<MediaSetItem>(entity =>
        {
            entity.Property(e => e.ExternalVideoUrl).HasMaxLength(500);

            entity.HasOne(d => d.File).WithMany(p => p.MediaSetItemFiles)
                .HasForeignKey(d => d.FileID)
                .HasConstraintName("FK_MediaSetItems_FileRecord");

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
            entity.ToTable("Member");

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

            entity.HasOne(d => d.Policy).WithMany(p => p.Members)
                .HasForeignKey(d => d.PolicyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Member_Policy");

            entity.HasOne(d => d.Website).WithMany(p => p.Members)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Member_Website");
        });

        modelBuilder.Entity<MemberForgetPassword>(entity =>
        {
            entity.ToTable("MemberForgetPassword");

            entity.Property(e => e.ForgetKey)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.LogTime).HasColumnType("datetime");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberForgetPasswords)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MemberForgetPassword_Member");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.ToTable("Menu");

            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Menu_IsActive");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Website).WithMany(p => p.Menus)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Menu_Website");
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.ToTable("MenuItem");

            entity.Property(e => e.CssClass).HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_MenuItem_IsActive");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Url).HasMaxLength(500);

            entity.HasOne(d => d.Brand).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.BrandID)
                .HasConstraintName("FK_MenuItem_Brands");

            entity.HasOne(d => d.Category).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.CategoryID)
                .HasConstraintName("FK_MenuItem_Category");

            entity.HasOne(d => d.Menu).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.MenuID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuItem_Menu");

            entity.HasOne(d => d.Page).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.PageID)
                .HasConstraintName("FK_MenuItem_Pages");

            entity.HasOne(d => d.ParentItem).WithMany(p => p.InverseParentItem)
                .HasForeignKey(d => d.ParentItemID)
                .HasConstraintName("FK_MenuItem_MenuItem");

            entity.HasOne(d => d.Post).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.PostID)
                .HasConstraintName("FK_MenuItem_Posts");

            entity.HasOne(d => d.ProductCategory).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.ProductCategoryID)
                .HasConstraintName("FK_MenuItem_ProductCategories");

            entity.HasOne(d => d.Product).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.ProductID)
                .HasConstraintName("FK_MenuItem_Products");

            entity.HasOne(d => d.Vendor).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.VendorID)
                .HasConstraintName("FK_MenuItem_Vendors");
        });

        modelBuilder.Entity<MenuItemTranslation>(entity =>
        {
            entity.ToTable("MenuItemTranslation");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.MenuItem).WithMany(p => p.MenuItemTranslations)
                .HasForeignKey(d => d.MenuItemID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuItemTranslation_MenuItem");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.AddressSnapshot).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Orders_CreatedAt_1")
                .HasColumnType("datetime");
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
            entity.Property(e => e.Status).HasDefaultValue((byte)1, "DF_Orders_Status_1");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.TaxTotal).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_Orders_Member");

            entity.HasOne(d => d.WebsiteClientAddress).WithMany(p => p.Orders)
                .HasForeignKey(d => d.WebsiteClientAddressID)
                .HasConstraintName("FK_Orders_WebsiteClientAddresses");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.Orders)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_WebsiteClient");

            entity.HasOne(d => d.Website).WithMany(p => p.Orders)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Website");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
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

            entity.HasOne(d => d.Website).WithMany(p => p.OrderItemWebsites)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Website");
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_OrderStatusHistories_CreatedAt")
                .HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(500);

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_OrderStatusHistories_Member");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.OrderID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderStatusHistories_Orders");
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Pages_CreatedAt_1");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Pages_IsActive_1");
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.Status).HasDefaultValue((byte)1, "DF_Pages_Status_1");
            entity.Property(e => e.Template).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Pages_UpdatedAt_1");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.Pages)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_Pages_Member");

            entity.HasOne(d => d.ParentPage).WithMany(p => p.InverseParentPage)
                .HasForeignKey(d => d.ParentPageID)
                .HasConstraintName("FK_Pages_Pages");

            entity.HasOne(d => d.Website).WithMany(p => p.Pages)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pages_Website");
        });

        modelBuilder.Entity<PageTranslation>(entity =>
        {
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
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.AmountUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Payments_CreatedAt_1");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ExchangeRateToUsd).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.GatewayRefNumber).HasMaxLength(100);
            entity.Property(e => e.PaidAt).HasPrecision(0);
            entity.Property(e => e.Status).HasDefaultValue((byte)1, "DF_Payments_Status_1");
            entity.Property(e => e.TrackingCode).HasMaxLength(100);

            entity.HasOne(d => d.BankAccount).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BankAccountID)
                .HasConstraintName("FK_Payments_BankAccounts");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderID)
                .HasConstraintName("FK_Payments_Orders");

            entity.HasOne(d => d.PaymentGateway).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentGatewayID)
                .HasConstraintName("FK_Payments_PaymentGateways");

            entity.HasOne(d => d.ReceiptFile).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ReceiptFileID)
                .HasConstraintName("FK_Payments_FileRecord");

            entity.HasOne(d => d.VerifiedByMember).WithMany(p => p.Payments)
                .HasForeignKey(d => d.VerifiedByMemberID)
                .HasConstraintName("FK_Payments_Member");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.Payments)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_WebsiteClient");

            entity.HasOne(d => d.Website).WithMany(p => p.Payments)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Website");
        });

        modelBuilder.Entity<PaymentGateway>(entity =>
        {
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.ApiSecret).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_PaymentGateways_IsActive");
            entity.Property(e => e.MerchantID).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Provider).HasMaxLength(50);

            entity.HasOne(d => d.Website).WithMany(p => p.PaymentGateways)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentGateways_Website");
        });

        modelBuilder.Entity<PaymentRefund>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_PaymentRefunds_CreatedAt")
                .HasColumnType("datetime");
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RefundedAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue((byte)1, "DF_PaymentRefunds_Status");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.PaymentRefunds)
                .HasForeignKey(d => d.BankAccountID)
                .HasConstraintName("FK_PaymentRefunds_BankAccounts");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.PaymentRefunds)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_PaymentRefunds_Member");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentRefunds)
                .HasForeignKey(d => d.PaymentID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentRefunds_Payments");
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.ToTable("Policy");

            entity.HasIndex(e => e.WebsiteID, "IX_Policy_WebsiteID");

            entity.Property(e => e.Title)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.WebsiteID).HasDefaultValue(1);

            entity.HasOne(d => d.Website).WithMany(p => p.Policies)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Policy_Website");
        });

        modelBuilder.Entity<PolicyRole>(entity =>
        {
            entity.ToTable("PolicyRole");

            entity.HasOne(d => d.Policy).WithMany(p => p.PolicyRoles)
                .HasForeignKey(d => d.PolicyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PolicyRole_Policy");

            entity.HasOne(d => d.Role).WithMany(p => p.PolicyRoles)
                .HasForeignKey(d => d.RoleID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PolicyRole_Role");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.Property(e => e.CommentsEnabled).HasDefaultValue(true, "DF_Posts_CommentsEnabled");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Posts_CreatedAt_1")
                .HasColumnType("datetime");
            entity.Property(e => e.Excerpt).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Posts_IsActive_1");
            entity.Property(e => e.PublishedAt).HasColumnType("datetime");
            entity.Property(e => e.ScheduledAt).HasColumnType("datetime");
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.Status).HasDefaultValue((byte)1, "DF_Posts_Status_1");
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Posts_UpdatedAt_1")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AuthorMember).WithMany(p => p.Posts)
                .HasForeignKey(d => d.AuthorMemberID)
                .HasConstraintName("FK_Posts_Member");

            entity.HasOne(d => d.FeaturedImageFile).WithMany(p => p.Posts)
                .HasForeignKey(d => d.FeaturedImageFileID)
                .HasConstraintName("FK_Posts_FileRecord");

            entity.HasOne(d => d.PostType).WithMany(p => p.Posts)
                .HasForeignKey(d => d.PostTypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Posts_PostType");

            entity.HasOne(d => d.Website).WithMany(p => p.Posts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Posts_Website");

            entity.HasMany(d => d.Tags).WithMany(p => p.Pos)
                .UsingEntity<Dictionary<string, object>>(
                    "PostTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PostTags_Tags"),
                    l => l.HasOne<Post>().WithMany()
                        .HasForeignKey("PosID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PostTags_Posts"),
                    j =>
                    {
                        j.HasKey("PosID", "TagID");
                        j.ToTable("PostTags");
                    });
        });

        modelBuilder.Entity<PostCategory>(entity =>
        {
            entity.HasKey(e => new { e.PostID, e.CategoryID });

            entity.ToTable("PostCategory");

            entity.HasOne(d => d.Category).WithMany(p => p.PostCategories)
                .HasForeignKey(d => d.CategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostCategory_Category");

            entity.HasOne(d => d.Post).WithMany(p => p.PostCategories)
                .HasForeignKey(d => d.PostID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostCategory_Posts");
        });

        modelBuilder.Entity<PostTranslation>(entity =>
        {
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
            entity.ToTable("PostType");

            entity.Property(e => e.CommentsEnabled).HasDefaultValue(true, "DF_PostType_CommentsEnabled");
            entity.Property(e => e.HasAuthor).HasDefaultValue(true, "DF_PostType_HasAuthor");
            entity.Property(e => e.HasCategories).HasDefaultValue(true, "DF_PostType_HasCategories");
            entity.Property(e => e.HasTags).HasDefaultValue(true, "DF_PostType_HasTags");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(100);

            entity.HasOne(d => d.Website).WithMany(p => p.PostTypes)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostType_Website");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.AvgRating).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Products_CreatedAt_1")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Products_IsActive_1");
            entity.Property(e => e.ShortDescription).HasMaxLength(1000);
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.Status).HasDefaultValue((byte)1, "DF_Products_Status_1");
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Products_UpdatedAt_1")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandID)
                .HasConstraintName("FK_Products_Brands");

            entity.HasOne(d => d.Website).WithMany(p => p.Products)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Website");
        });

        modelBuilder.Entity<ProductAnswer>(entity =>
        {
            entity.Property(e => e.Body).HasMaxLength(4000);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_ProductAnswers_CreatedAt")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue((byte)1, "DF_ProductAnswers_Status");

            entity.HasOne(d => d.ProductQuestion).WithMany(p => p.ProductAnswers)
                .HasForeignKey(d => d.ProductQuestionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductAnswers_ProductQuestions");

            entity.HasOne(d => d.Vendor).WithMany(p => p.ProductAnswers)
                .HasForeignKey(d => d.VendorID)
                .HasConstraintName("FK_ProductAnswers_Vendors");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.ProductAnswers)
                .HasForeignKey(d => d.WebsiteClientID)
                .HasConstraintName("FK_ProductAnswers_WebsiteClient");
        });

        modelBuilder.Entity<ProductAttributeValue>(entity =>
        {
            entity.Property(e => e.CustomValue).HasMaxLength(1000);
            entity.Property(e => e.NumericValue).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.AttributeDefinition).WithMany(p => p.ProductAttributeValues)
                .HasForeignKey(d => d.AttributeDefinitionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductAttributeValues_AttributeDefinition");

            entity.HasOne(d => d.AttributeOption).WithMany(p => p.ProductAttributeValues)
                .HasForeignKey(d => d.AttributeOptionID)
                .HasConstraintName("FK_ProductAttributeValues_AttributeOption");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAttributeValues)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductAttributeValues_Products");
        });

        modelBuilder.Entity<ProductAttributeValueTranslation>(entity =>
        {
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
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_ProductCategories_IsActive");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);

            entity.HasOne(d => d.ImageFile).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.ImageFileID)
                .HasConstraintName("FK_ProductCategories_FileRecord");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryID)
                .HasConstraintName("FK_ProductCategories_ProductCategories");

            entity.HasOne(d => d.Website).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductCategories_Website");
        });

        modelBuilder.Entity<ProductCategoryMap>(entity =>
        {
            entity.HasKey(e => new { e.ProductID, e.ProductCategoryID });

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

            entity.Property(e => e.MaxItems).HasDefaultValue(10, "DF_ProductCategoryRelations_MaxItems");

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

        modelBuilder.Entity<ProductContentSection>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_ProductContentSections_IsActive");

            entity.HasOne(d => d.MediaSet).WithMany(p => p.ProductContentSections)
                .HasForeignKey(d => d.MediaSetID)
                .HasConstraintName("FK_ProductContentSections_MediaSets");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductContentSections)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductContentSections_Products");
        });

        modelBuilder.Entity<ProductContentSectionTranslation>(entity =>
        {
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.ProductContentSection).WithMany(p => p.ProductContentSectionTranslations)
                .HasForeignKey(d => d.ProductContentSectionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductContentSectionTranslations_ProductContentSections");
        });

        modelBuilder.Entity<ProductMedium>(entity =>
        {
            entity.HasKey(e => new { e.ProductID, e.MediaSetID });

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
            entity.Property(e => e.Body).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_ProductQuestions_CreatedAt")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue((byte)1, "DF_ProductQuestions_Status");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductQuestions)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductQuestions_Products");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.ProductQuestions)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductQuestions_WebsiteClient");

            entity.HasOne(d => d.Website).WithMany(p => p.ProductQuestions)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductQuestions_Website");
        });

        modelBuilder.Entity<ProductRelation>(entity =>
        {
            entity.HasKey(e => new { e.ProductID, e.RelatedProductID, e.RelationType });

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
            entity.Property(e => e.ConsJson).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_ProductReviews_CreatedAt");
            entity.Property(e => e.ProsJson).HasMaxLength(2000);
            entity.Property(e => e.Status).HasDefaultValue((byte)1, "DF_ProductReviews_Status");
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
                .HasConstraintName("FK_ProductReviews_WebsiteClient");

            entity.HasOne(d => d.Website).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductReviews_Website");
        });

        modelBuilder.Entity<ProductTranslation>(entity =>
        {
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
            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.CompareAtPriceUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_ProductVariants_CreatedAt")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_ProductVariants_IsActive");
            entity.Property(e => e.OverridePrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ReferencePriceUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Sku).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("decimal(10, 3)");

            entity.HasOne(d => d.ImageFile).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ImageFileID)
                .HasConstraintName("FK_ProductVariants_FileRecord");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductVariants_Products");

            entity.HasOne(d => d.Website).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductVariants_Website");
        });

        modelBuilder.Entity<ProductWarning>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_ProductWarnings_IsActive");
            entity.Property(e => e.Severity)
                .HasMaxLength(20)
                .HasDefaultValue("info", "DF_ProductWarnings_Severity");
            entity.Property(e => e.Text).HasMaxLength(1000);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductWarnings)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductWarnings_Products");
        });

        modelBuilder.Entity<ProductWarningTranslation>(entity =>
        {
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

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.Description)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.RoleKey)
                .HasMaxLength(42)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Settlements_CreatedAt_1")
                .HasColumnType("datetime");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentRefNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasDefaultValue((byte)1, "DF_Settlements_Status_1");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.ApprovedByMember).WithMany(p => p.SettlementApprovedByMembers)
                .HasForeignKey(d => d.ApprovedByMemberID)
                .HasConstraintName("FK_Settlements_Member1");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.BankAccountID)
                .HasConstraintName("FK_Settlements_BankAccounts");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.SettlementCreatedByMembers)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_Settlements_Member");

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
                .HasConstraintName("FK_Settlements_Website");
        });

        modelBuilder.Entity<SettlementItem>(entity =>
        {
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

        modelBuilder.Entity<State>(entity =>
        {
            entity.ToTable("State");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Tile).HasMaxLength(48);

            entity.HasOne(d => d.Country).WithMany(p => p.States)
                .HasForeignKey(d => d.CountryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_State_Country");
        });

        modelBuilder.Entity<StateTranslation>(entity =>
        {
            entity.ToTable("StateTranslation");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Tile).HasMaxLength(48);

            entity.HasOne(d => d.State).WithMany(p => p.StateTranslations)
                .HasForeignKey(d => d.StateID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StateTranslation_State");
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_StockMovements_CreatedAt");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.UnitCostUsd).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.UnitSalePriceUsd).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.CreatedByMemberID)
                .HasConstraintName("FK_StockMovements_Member");

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
                .HasConstraintName("FK_StockMovements_Website");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Suppliers_IsActive_1");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.Website).WithMany(p => p.Suppliers)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Suppliers_Website");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Slug).HasMaxLength(150);

            entity.HasOne(d => d.Website).WithMany(p => p.Tags)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tags_Website");
        });

        modelBuilder.Entity<TagTranslation>(entity =>
        {
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

        modelBuilder.Entity<VariantAttributeValue>(entity =>
        {
            entity.HasKey(e => new { e.ProductVariantID, e.AttributeDefinitionID });

            entity.HasOne(d => d.AttributeDefinition).WithMany(p => p.VariantAttributeValues)
                .HasForeignKey(d => d.AttributeDefinitionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VariantAttributeValues_AttributeDefinition");

            entity.HasOne(d => d.AttributeOption).WithMany(p => p.VariantAttributeValues)
                .HasForeignKey(d => d.AttributeOptionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VariantAttributeValues_AttributeOption");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.VariantAttributeValues)
                .HasForeignKey(d => d.ProductVariantID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VariantAttributeValues_ProductVariants");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Vendors_IsActive_1");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Rating).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.Slug).HasMaxLength(200);

            entity.HasOne(d => d.LogoFile).WithMany(p => p.Vendors)
                .HasForeignKey(d => d.LogoFileID)
                .HasConstraintName("FK_Vendors_FileRecord");

            entity.HasOne(d => d.Website).WithMany(p => p.Vendors)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vendors_Website");
        });

        modelBuilder.Entity<VendorProduct>(entity =>
        {
            entity.Property(e => e.DeliveryDays).HasDefaultValue(1, "DF_VendorProducts_DeliveryDays");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_VendorProducts_IsActive_1");
            entity.Property(e => e.OverridePrice).HasColumnType("decimal(18, 4)");
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
                .HasConstraintName("FK_VendorProducts_Website");
        });

        modelBuilder.Entity<VendorTranslation>(entity =>
        {
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

        modelBuilder.Entity<Website>(entity =>
        {
            entity.ToTable("Website");

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
            entity.Property(e => e.TradeName).HasMaxLength(32);
            entity.Property(e => e.WebsiteAddress)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.HasOne(d => d.FaveIconFile).WithMany(p => p.WebsiteFaveIconFiles)
                .HasForeignKey(d => d.FaveIconFileID)
                .HasConstraintName("FK_Website_FileRecord1");

            entity.HasOne(d => d.LogoFile).WithMany(p => p.WebsiteLogoFiles)
                .HasForeignKey(d => d.LogoFileID)
                .HasConstraintName("FK_Website_FileRecord");
        });

        modelBuilder.Entity<WebsiteClient>(entity =>
        {
            entity.ToTable("WebsiteClient");

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
                .HasConstraintName("FK_WebsiteClient_FileRecord");

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteClients)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteClient_Website");
        });

        modelBuilder.Entity<WebsiteClientAddress>(entity =>
        {
            entity.Property(e => e.AddressLine).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.ReceiverName).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.City).WithMany(p => p.WebsiteClientAddresses)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_WebsiteClientAddresses_City");

            entity.HasOne(d => d.Country).WithMany(p => p.WebsiteClientAddresses)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("FK_WebsiteClientAddresses_Country");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.WebsiteClientAddresses)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteClientAddresses_WebsiteClient");
        });

        modelBuilder.Entity<WebsiteClientForgetPassword>(entity =>
        {
            entity.ToTable("WebsiteClientForgetPassword");

            entity.Property(e => e.ForgetKey)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.LogTime).HasColumnType("datetime");

            entity.HasOne(d => d.WebsiteClient).WithMany(p => p.WebsiteClientForgetPasswords)
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteClientForgetPassword_WebsiteClient");
        });

        modelBuilder.Entity<WebsiteIP>(entity =>
        {
            entity.ToTable("WebsiteIP");

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
                .HasConstraintName("FK_WebsiteIP_Website");
        });

        modelBuilder.Entity<WebsiteRedirect>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_WebsiteRedirects_CreatedAt")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_WebsiteRedirects_IsActive");
            entity.Property(e => e.SourcePath).HasMaxLength(500);
            entity.Property(e => e.StatusCode).HasDefaultValue(301, "DF_WebsiteRedirects_StatusCode");
            entity.Property(e => e.TargetPath).HasMaxLength(500);

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteRedirects)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteRedirects_Website");
        });

        modelBuilder.Entity<WebsiteScript>(entity =>
        {
            entity.ToTable("WebsiteScript");

            entity.Property(e => e.LogTime).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RawContent).HasMaxLength(4000);

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteScripts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteScript_Website");
        });

        modelBuilder.Entity<WebsiteSeoSetting>(entity =>
        {
            entity.ToTable("WebsiteSeoSetting");

            entity.Property(e => e.DefaultMetaDescription).HasMaxLength(158);
            entity.Property(e => e.DefaultMetaTitle)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasComment("Page | {SiteName}");
            entity.Property(e => e.TitleSeparator)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasDefaultValue(" | \" or \" - \" or \" › ", "DF_WebsiteSeoSetting_TitleSeparator");

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteSeoSettings)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteSeoSetting_Website");
        });

        modelBuilder.Entity<WebsiteSocialLink>(entity =>
        {
            entity.ToTable("WebsiteSocialLink");

            entity.Property(e => e.SocialIcon)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.SocialName).HasMaxLength(64);
            entity.Property(e => e.UrlAddress).HasMaxLength(80);

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteSocialLinks)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteSocialLink_Website");
        });

        modelBuilder.Entity<WebstieStorageSetting>(entity =>
        {
            entity.HasKey(e => e.WebsiteStorageSettingsID);

            entity.Property(e => e.AllowedExtensions)
                .HasMaxLength(710)
                .IsUnicode(false);
            entity.Property(e => e.StorageSettingsJSON).HasMaxLength(2000);

            entity.HasOne(d => d.Website).WithMany(p => p.WebstieStorageSettings)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebstieStorageSettings_Website");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
