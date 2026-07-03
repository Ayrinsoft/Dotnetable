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

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryTranslation> CategoryTranslations { get; set; }

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

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<LocalizationKey> LocalizationKeys { get; set; }

    public virtual DbSet<LocalizationValue> LocalizationValues { get; set; }

    public virtual DbSet<LoginTry> LoginTries { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<MemberForgetPassword> MemberForgetPasswords { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MenuItem> MenuItems { get; set; }

    public virtual DbSet<MenuItemTranslation> MenuItemTranslations { get; set; }

    public virtual DbSet<Page> Pages { get; set; }

    public virtual DbSet<PageTranslation> PageTranslations { get; set; }

    public virtual DbSet<Policy> Policies { get; set; }

    public virtual DbSet<PolicyRole> PolicyRoles { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostCategory> PostCategories { get; set; }

    public virtual DbSet<PostTranslation> PostTranslations { get; set; }

    public virtual DbSet<PostType> PostTypes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<StateTranslation> StateTranslations { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TagTranslation> TagTranslations { get; set; }

    public virtual DbSet<Website> Websites { get; set; }

    public virtual DbSet<WebsiteClient> WebsiteClients { get; set; }

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
            entity.ToTable("BankAccount");

            entity.Property(e => e.AccountNumber)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CardNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.IBAN)
                .HasMaxLength(34)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_BankAccount_IsActive");
            entity.Property(e => e.IsForOfflinePayment).HasDefaultValue(true, "DF_BankAccount_IsForOfflinePayment");
            entity.Property(e => e.JSONSettings)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.OwnerName).HasMaxLength(90);
            entity.Property(e => e.Title).HasMaxLength(70);

            entity.HasOne(d => d.Bank).WithMany(p => p.BankAccounts)
                .HasForeignKey(d => d.BankID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BankAccount_Bank");

            entity.HasOne(d => d.CreatedByMember).WithMany(p => p.BankAccounts)
                .HasForeignKey(d => d.CreatedByMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BankAccount_Member");

            entity.HasOne(d => d.Website).WithMany(p => p.BankAccounts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BankAccount_Website");
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
