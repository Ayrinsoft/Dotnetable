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

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<CityTranslation> CityTranslations { get; set; }

    public virtual DbSet<ContactUsMessage> ContactUsMessages { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<CountryTranslation> CountryTranslations { get; set; }

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

    public virtual DbSet<Policy> Policies { get; set; }

    public virtual DbSet<PolicyRole> PolicyRoles { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<StateTranslation> StateTranslations { get; set; }

    public virtual DbSet<Website> Websites { get; set; }

    public virtual DbSet<WebsiteClient> WebsiteClients { get; set; }

    public virtual DbSet<WebsiteClientForgetPassword> WebsiteClientForgetPasswords { get; set; }

    public virtual DbSet<WebsiteIP> WebsiteIPs { get; set; }

    public virtual DbSet<WebsiteScript> WebsiteScripts { get; set; }

    public virtual DbSet<WebsiteSeoSetting> WebsiteSeoSettings { get; set; }

    public virtual DbSet<WebsiteSocialLink> WebsiteSocialLinks { get; set; }

    public virtual DbSet<WebstieStorageSetting> WebstieStorageSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        modelBuilder.Entity<Website>(entity =>
        {
            entity.ToTable("Website");

            entity.Property(e => e.BrandName)
                .HasMaxLength(60)
                .HasComment("show in title of pages");
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
