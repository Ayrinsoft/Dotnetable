using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dotnetable.Data.DBContext;

public partial class DotnetableEntity : DbContext
{
    public DotnetableEntity()
    {
    }

    public DotnetableEntity(DbContextOptions<DotnetableEntity> options)
        : base(options)
    {
    }

    public virtual DbSet<TBM_Member_File> TBM_Member_Files { get; set; }

    public virtual DbSet<TBM_Policy_Role> TBM_Policy_Roles { get; set; }

    public virtual DbSet<TBM_Post_Category_Relation> TBM_Post_Category_Relations { get; set; }

    public virtual DbSet<TBM_Post_File> TBM_Post_Files { get; set; }

    public virtual DbSet<TB_City> TB_Cities { get; set; }

    public virtual DbSet<TB_City_Language> TB_City_Languages { get; set; }

    public virtual DbSet<TB_Comment_Category> TB_Comment_Categories { get; set; }

    public virtual DbSet<TB_Comment_Type> TB_Comment_Types { get; set; }

    public virtual DbSet<TB_ContactUs_Message> TB_ContactUs_Messages { get; set; }

    public virtual DbSet<TB_Country> TB_Countries { get; set; }

    public virtual DbSet<TB_Country_Language> TB_Country_Languages { get; set; }

    public virtual DbSet<TB_Email_Setting> TB_Email_Settings { get; set; }

    public virtual DbSet<TB_Email_Subscribe> TB_Email_Subscribes { get; set; }

    public virtual DbSet<TB_Email_Type> TB_Email_Types { get; set; }

    public virtual DbSet<TB_File> TB_Files { get; set; }

    public virtual DbSet<TB_File_Category> TB_File_Categories { get; set; }

    public virtual DbSet<TB_File_Temp_Store> TB_File_Temp_Stores { get; set; }

    public virtual DbSet<TB_File_Type> TB_File_Types { get; set; }

    public virtual DbSet<TB_IP_Address_Action> TB_IP_Address_Actions { get; set; }

    public virtual DbSet<TB_Language> TB_Languages { get; set; }

    public virtual DbSet<TB_Login_Token> TB_Login_Tokens { get; set; }

    public virtual DbSet<TB_Login_Try> TB_Login_Tries { get; set; }

    public virtual DbSet<TB_Member> TB_Members { get; set; }

    public virtual DbSet<TB_Member_Activate_Log> TB_Member_Activate_Logs { get; set; }

    public virtual DbSet<TB_Member_Contact> TB_Member_Contacts { get; set; }

    public virtual DbSet<TB_Member_Forget_Password> TB_Member_Forget_Passwords { get; set; }

    public virtual DbSet<TB_Page_Setting> TB_Page_Settings { get; set; }

    public virtual DbSet<TB_Policy> TB_Policies { get; set; }

    public virtual DbSet<TB_Post> TB_Posts { get; set; }

    public virtual DbSet<TB_Post_Category> TB_Post_Categories { get; set; }

    public virtual DbSet<TB_Post_Category_Language> TB_Post_Category_Languages { get; set; }

    public virtual DbSet<TB_Post_Comment> TB_Post_Comments { get; set; }

    public virtual DbSet<TB_Post_Language> TB_Post_Languages { get; set; }

    public virtual DbSet<TB_Role> TB_Roles { get; set; }

    public virtual DbSet<TB_Setting> TB_Settings { get; set; }

    public virtual DbSet<TB_SlideShow> TB_SlideShows { get; set; }

    public virtual DbSet<TB_SlideShow_Language> TB_SlideShow_Languages { get; set; }

    public virtual DbSet<TB_State> TB_States { get; set; }

    public virtual DbSet<TB_State_Language> TB_State_Languages { get; set; }

    private string DBType { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            base.OnConfiguring(optionsBuilder);
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot config = builder.Build();
            DBType = config.GetSection("DBConfiguration").GetChildren().FirstOrDefault(i => i.Key == "MainDBType")?.Value ?? "MSSQL";

            switch (DBType)
            {
                case "MYSQL":
                    optionsBuilder.UseMySQL(config.GetConnectionString("DotnetableConnection"));
                    break;
                case "MARIADB":
                    string DBVersion = config.GetSection("DBConfiguration").GetChildren().FirstOrDefault(i => i.Key == "Version")?.Value ?? "";
                    optionsBuilder.UseMySql(config.GetConnectionString("DotnetableConnection"), new MySqlServerVersion(DBVersion));
                    break;
                case "POSTGRESQL":
                    optionsBuilder.UseNpgsql(config.GetConnectionString("DotnetableConnection"), o => o.UseNodaTime());
                    break;
                default:
                    optionsBuilder.UseSqlServer(config.GetConnectionString("DotnetableConnection"));
                    break;
            }
        }
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TBM_Member_File>(entity =>
        {
            entity.HasKey(e => e.MemberFileID);

            entity.ToTable("TBM_Member_File");

            entity.HasOne(d => d.File).WithMany(p => p.TBM_Member_Files)
                .HasForeignKey(d => d.FileID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBM_Member_File_TB_File");

            entity.HasOne(d => d.Member).WithMany(p => p.TBM_Member_Files)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBM_Member_File_TB_Member");
        });

        modelBuilder.Entity<TBM_Policy_Role>(entity =>
        {
            entity.HasKey(e => e.PolicyRoleID);

            entity.ToTable("TBM_Policy_Role");

            entity.HasOne(d => d.Policy).WithMany(p => p.TBM_Policy_Roles)
                .HasForeignKey(d => d.PolicyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBM_Policy_Role_TB_Policy");

            entity.HasOne(d => d.Role).WithMany(p => p.TBM_Policy_Roles)
                .HasForeignKey(d => d.RoleID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBM_Policy_Role_TB_Role");
        });

        modelBuilder.Entity<TBM_Post_Category_Relation>(entity =>
        {
            entity.HasKey(e => e.PostCategoryRelationID);

            entity.ToTable("TBM_Post_Category_Relation");

            entity.HasOne(d => d.PostCategory).WithMany(p => p.TBM_Post_Category_Relations)
                .HasForeignKey(d => d.PostCategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBM_Post_Category_Relation_TB_Post_Category");

            entity.HasOne(d => d.Post).WithMany(p => p.TBM_Post_Category_Relations)
                .HasForeignKey(d => d.PostID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBM_Post_Category_Relation_TB_Post");
        });

        modelBuilder.Entity<TBM_Post_File>(entity =>
        {
            entity.HasKey(e => e.PostFileID);

            entity.ToTable("TBM_Post_File");

            entity.HasOne(d => d.File).WithMany(p => p.TBM_Post_Files)
                .HasForeignKey(d => d.FileID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBM_Post_File_TB_File");

            entity.HasOne(d => d.Post).WithMany(p => p.TBM_Post_Files)
                .HasForeignKey(d => d.PostID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBM_Post_File_TB_Post");
        });

        modelBuilder.Entity<TB_City>(entity =>
        {
            entity.HasKey(e => e.CityID);

            entity.ToTable("TB_City");

            entity.Property(e => e.LanguageCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(48);

            entity.HasOne(d => d.Country).WithMany(p => p.TB_Cities)
                .HasForeignKey(d => d.CountryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_City_TB_Country");

            entity.HasOne(d => d.State).WithMany(p => p.TB_Cities)
                .HasForeignKey(d => d.StateID)
                .HasConstraintName("FK_TB_City_TB_State");
        });

        modelBuilder.Entity<TB_City_Language>(entity =>
        {
            entity.HasKey(e => e.CityLanguageID);

            entity.ToTable("TB_City_Language");

            entity.Property(e => e.LanguageCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(48);

            entity.HasOne(d => d.City).WithMany(p => p.TB_City_Languages)
                .HasForeignKey(d => d.CityID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_City_Language_TB_City");
        });

        modelBuilder.Entity<TB_Comment_Category>(entity =>
        {
            entity.HasKey(e => e.CommentCategoryID);

            entity.ToTable("TB_Comment_Category");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(8)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TB_Comment_Type>(entity =>
        {
            entity.HasKey(e => e.CommentTypeID);

            entity.ToTable("TB_Comment_Type");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(16)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TB_ContactUs_Message>(entity =>
        {
            entity.HasKey(e => e.ContactUsMessagesID);

            entity.ToTable("TB_ContactUs_Message");

            entity.Property(e => e.CellphoneNumber)
                .IsRequired()
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.EmailAddress)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.LogTime).HasColumnType("datetime");
            entity.Property(e => e.MessageBody)
                .IsRequired()
                .HasMaxLength(4000);
            entity.Property(e => e.MessageSubject)
                .IsRequired()
                .HasMaxLength(512);
            entity.Property(e => e.SenderIPAddress)
                .IsRequired()
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SenderName)
                .IsRequired()
                .HasMaxLength(64);
        });

        modelBuilder.Entity<TB_Country>(entity =>
        {
            entity.HasKey(e => e.CountryID);

            entity.ToTable("TB_Country");

            entity.Property(e => e.CountryID).ValueGeneratedOnAdd();
            entity.Property(e => e.CountryCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LanguageCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PhonePerfix)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(42);
        });

        modelBuilder.Entity<TB_Country_Language>(entity =>
        {
            entity.HasKey(e => e.CountryLanguageID);

            entity.ToTable("TB_Country_Language");

            entity.Property(e => e.LanguageCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(42);

            entity.HasOne(d => d.Country).WithMany(p => p.TB_Country_Languages)
                .HasForeignKey(d => d.CountryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Country_Language_TB_Country");
        });

        modelBuilder.Entity<TB_Email_Setting>(entity =>
        {
            entity.HasKey(e => e.EmailSettingID);

            entity.ToTable("TB_Email_Setting");

            entity.Property(e => e.EmailAddress)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.MailName)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.MailServer)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasOne(d => d.EmailType).WithMany(p => p.TB_Email_Settings)
                .HasForeignKey(d => d.EmailTypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Email_Setting_TB_Email_Type");
        });

        modelBuilder.Entity<TB_Email_Subscribe>(entity =>
        {
            entity.HasKey(e => e.EmailSubscribeID);

            entity.ToTable("TB_Email_Subscribe");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.LogTime).HasColumnType("datetime");

            entity.HasOne(d => d.Member).WithMany(p => p.TB_Email_Subscribes)
                .HasForeignKey(d => d.MemberID)
                .HasConstraintName("FK_TB_Email_Subscribe_TB_Member");
        });

        modelBuilder.Entity<TB_Email_Type>(entity =>
        {
            entity.HasKey(e => e.EmailTypeID);

            entity.ToTable("TB_Email_Type");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(8)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TB_File>(entity =>
        {
            entity.HasKey(e => e.FileID);

            entity.ToTable("TB_File");

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.FilePath)
                .IsRequired()
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.UploadTime).HasColumnType("datetime");

            entity.HasOne(d => d.FileCategory).WithMany(p => p.TB_Files)
                .HasForeignKey(d => d.FileCategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_File_TB_File_Category");

            entity.HasOne(d => d.FileType).WithMany(p => p.TB_Files)
                .HasForeignKey(d => d.FileTypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_File_TB_File_Type");

            entity.HasOne(d => d.Uploader).WithMany(p => p.TB_Files)
                .HasForeignKey(d => d.UploaderID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_File_TB_Member");
        });

        modelBuilder.Entity<TB_File_Category>(entity =>
        {
            entity.HasKey(e => e.FileCategoryID);

            entity.ToTable("TB_File_Category");

            entity.Property(e => e.Tite)
                .IsRequired()
                .HasMaxLength(32)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TB_File_Temp_Store>(entity =>
        {
            entity.HasKey(e => e.FileTempID).HasName("PK_TB_T_File");

            entity.ToTable("TB_File_Temp_Store");

            entity.Property(e => e.ExpireTime).HasColumnType("datetime");

            entity.HasOne(d => d.File).WithMany(p => p.TB_File_Temp_Stores)
                .HasForeignKey(d => d.FileID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_T_File_TB_File");
        });

        modelBuilder.Entity<TB_File_Type>(entity =>
        {
            entity.HasKey(e => e.FileTypeID);

            entity.ToTable("TB_File_Type");

            entity.Property(e => e.FileExtention)
                .IsRequired()
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.FileTypeName)
                .IsRequired()
                .HasMaxLength(78)
                .IsUnicode(false);
            entity.Property(e => e.MIMEType)
                .IsRequired()
                .HasMaxLength(73)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TB_IP_Address_Action>(entity =>
        {
            entity.HasKey(e => e.IPAddressActionID);

            entity.ToTable("TB_IP_Address_Action");

            entity.Property(e => e.LogTime).HasColumnType("datetime");
            entity.Property(e => e.TryIP)
                .IsRequired()
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TB_Language>(entity =>
        {
            entity.HasKey(e => e.LanguageID);

            entity.ToTable("TB_Language");

            entity.Property(e => e.LanguageID).ValueGeneratedOnAdd();
            entity.Property(e => e.LanguageCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LanguageISOCode)
                .IsRequired()
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.LocalizedTitle)
                .IsRequired()
                .HasMaxLength(32);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(32)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TB_Login_Token>(entity =>
        {
            entity.HasKey(e => e.LoginTokenID).HasName("PK_TB_L_Login_Token");

            entity.ToTable("TB_Login_Token");

            entity.Property(e => e.ExpireTime).HasColumnType("datetime");

            entity.HasOne(d => d.Member).WithMany(p => p.TB_Login_Tokens)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_L_Login_Token_TB_Member");
        });

        modelBuilder.Entity<TB_Login_Try>(entity =>
        {
            entity.HasKey(e => e.LoginTryID);

            entity.ToTable("TB_Login_Try");

            entity.Property(e => e.LogTime).HasColumnType("datetime");
            entity.Property(e => e.TryIP)
                .IsRequired()
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TB_Member>(entity =>
        {
            entity.HasKey(e => e.MemberID);

            entity.ToTable("TB_Member");

            entity.Property(e => e.CellphoneNumber)
                .IsRequired()
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.CountryCode)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Givenname)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.PostalCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.RegisterDate).HasColumnType("datetime");
            entity.Property(e => e.Surname)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.HasOne(d => d.City).WithMany(p => p.TB_Members)
                .HasForeignKey(d => d.CityID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Member_TB_City");

            entity.HasOne(d => d.Policy).WithMany(p => p.TB_Members)
                .HasForeignKey(d => d.PolicyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Member_TB_Policy");
        });

        modelBuilder.Entity<TB_Member_Activate_Log>(entity =>
        {
            entity.HasKey(e => e.MemberActivateLogID);

            entity.ToTable("TB_Member_Activate_Log");

            entity.Property(e => e.ExpireDate).HasColumnType("datetime");

            entity.HasOne(d => d.Member).WithMany(p => p.TB_Member_Activate_Logs)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Member_Activate_Log_TB_Member_Activate_Log");
        });

        modelBuilder.Entity<TB_Member_Contact>(entity =>
        {
            entity.HasKey(e => e.MemberContactID);

            entity.ToTable("TB_Member_Contact");

            entity.Property(e => e.Address).HasMaxLength(800);
            entity.Property(e => e.CellphoneNumber)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.HomeOwnerName).HasMaxLength(70);
            entity.Property(e => e.LanguageCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.PointLatitude)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.PointLongitude)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.PostalCode)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.City).WithMany(p => p.TB_Member_Contacts)
                .HasForeignKey(d => d.CityID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Member_Contact_TB_City");

            entity.HasOne(d => d.Member).WithMany(p => p.TB_Member_Contacts)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Member_Contact_TB_Member");
        });

        modelBuilder.Entity<TB_Member_Forget_Password>(entity =>
        {
            entity.HasKey(e => e.MemberForgetPasswordID);

            entity.ToTable("TB_Member_Forget_Password");

            entity.Property(e => e.ForgetKey)
                .IsRequired()
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.LogTime).HasColumnType("datetime");

            entity.HasOne(d => d.Member).WithMany(p => p.TB_Member_Forget_Passwords)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Member_Forget_Password_TB_Member");
        });

        modelBuilder.Entity<TB_Page_Setting>(entity =>
        {
            entity.HasKey(e => e.PageSettingsID);

            entity.Property(e => e.ItemBody)
                .IsRequired()
                .HasMaxLength(4000);
            entity.Property(e => e.ItemTag)
                .IsRequired()
                .HasMaxLength(32)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TB_Policy>(entity =>
        {
            entity.HasKey(e => e.PolicyID);

            entity.ToTable("TB_Policy");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TB_Post>(entity =>
        {
            entity.HasKey(e => e.PostID);

            entity.ToTable("TB_Post");

            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LogTime).HasColumnType("datetime");
            entity.Property(e => e.MetaDescription).HasMaxLength(255);
            entity.Property(e => e.MetaKeywords).HasMaxLength(255);
            entity.Property(e => e.PostCode)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Summary)
                .IsRequired()
                .HasMaxLength(1024);
            entity.Property(e => e.Tags).HasMaxLength(2000);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(512);

            entity.HasOne(d => d.Member).WithMany(p => p.TB_Posts)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Post_TB_Member");

            entity.HasOne(d => d.PostCategory).WithMany(p => p.TB_Posts)
                .HasForeignKey(d => d.PostCategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Post_TB_Post_Category");
        });

        modelBuilder.Entity<TB_Post_Category>(entity =>
        {
            entity.HasKey(e => e.PostCategoryID);

            entity.ToTable("TB_Post_Category");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(512);
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MetaDescription).HasMaxLength(256);
            entity.Property(e => e.MetaKeywords).HasMaxLength(256);
            entity.Property(e => e.Tags).HasMaxLength(2000);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(128);
        });

        modelBuilder.Entity<TB_Post_Category_Language>(entity =>
        {
            entity.HasKey(e => e.PostCategoryLanguageID).HasName("PK_TB_L_Post_Category");

            entity.ToTable("TB_Post_Category_Language");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(512);
            entity.Property(e => e.LanguageCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MetaDescription).HasMaxLength(256);
            entity.Property(e => e.MetaKeywords).HasMaxLength(256);
            entity.Property(e => e.Tags).HasMaxLength(2000);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasOne(d => d.PostCategory).WithMany(p => p.TB_Post_Category_Languages)
                .HasForeignKey(d => d.PostCategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_L_Post_Category_TB_Post_Category");
        });

        modelBuilder.Entity<TB_Post_Comment>(entity =>
        {
            entity.HasKey(e => e.PostCommentID);

            entity.ToTable("TB_Post_Comment");

            entity.Property(e => e.CommentBody)
                .IsRequired()
                .HasMaxLength(512);
            entity.Property(e => e.IPAddress)
                .IsRequired()
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.LanguageCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LogTime).HasColumnType("datetime");

            entity.HasOne(d => d.CommentType).WithMany(p => p.TB_Post_Comments)
                .HasForeignKey(d => d.CommentTypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Post_Comment_TB_Comment_Type");

            entity.HasOne(d => d.Member).WithMany(p => p.TB_Post_Comments)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Post_Comment_TB_Member");

            entity.HasOne(d => d.Post).WithMany(p => p.TB_Post_Comments)
                .HasForeignKey(d => d.PostID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Post_Comment_TB_Post");

            entity.HasOne(d => d.ReplyPostComment).WithMany(p => p.InverseReplyPostComment)
                .HasForeignKey(d => d.ReplyPostCommentID)
                .HasConstraintName("FK_TB_Post_Comment_TB_Post_Comment");
        });

        modelBuilder.Entity<TB_Post_Language>(entity =>
        {
            entity.HasKey(e => e.PostLanguageID).HasName("PK_TB_L_Post");

            entity.ToTable("TB_Post_Language");

            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.LanguageCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MetaDescription).HasMaxLength(255);
            entity.Property(e => e.MetaKeywords).HasMaxLength(255);
            entity.Property(e => e.Summary)
                .IsRequired()
                .HasMaxLength(1024);
            entity.Property(e => e.Tags).HasMaxLength(2000);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(512);

            entity.HasOne(d => d.Post).WithMany(p => p.TB_Post_Languages)
                .HasForeignKey(d => d.PostID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_Post_Language_TB_Post");
        });

        modelBuilder.Entity<TB_Role>(entity =>
        {
            entity.HasKey(e => e.RoleID);

            entity.ToTable("TB_Role");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.RoleKey)
                .IsRequired()
                .HasMaxLength(42)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TB_Setting>(entity =>
        {
            entity.HasKey(e => e.SettingID);

            entity.ToTable("TB_Setting");

            entity.Property(e => e.SettingKey)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.SettingValue)
                .IsRequired()
                .HasMaxLength(4000);
        });

        modelBuilder.Entity<TB_SlideShow>(entity =>
        {
            entity.HasKey(e => e.SlideShowID);

            entity.ToTable("TB_SlideShow");

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PageCode)
                .IsRequired()
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.SettingArray).IsRequired();
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(64);
        });

        modelBuilder.Entity<TB_SlideShow_Language>(entity =>
        {
            entity.HasKey(e => e.SlideShowLanguageID);

            entity.ToTable("TB_SlideShow_Language");

            entity.Property(e => e.LanguageCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SettingArray).IsRequired();
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(64);

            entity.HasOne(d => d.SlideShow).WithMany(p => p.TB_SlideShow_Languages)
                .HasForeignKey(d => d.SlideShowID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_SlideShow_Language_TB_SlideShow");
        });

        modelBuilder.Entity<TB_State>(entity =>
        {
            entity.HasKey(e => e.StateID);

            entity.ToTable("TB_State");

            entity.Property(e => e.LanguageCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Tile)
                .IsRequired()
                .HasMaxLength(48);

            entity.HasOne(d => d.Country).WithMany(p => p.TB_States)
                .HasForeignKey(d => d.CountryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_State_TB_Country");
        });

        modelBuilder.Entity<TB_State_Language>(entity =>
        {
            entity.HasKey(e => e.StateLanguageID);

            entity.ToTable("TB_State_Language");

            entity.Property(e => e.LanguageCode)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Tile)
                .IsRequired()
                .HasMaxLength(48);

            entity.HasOne(d => d.State).WithMany(p => p.TB_State_Languages)
                .HasForeignKey(d => d.StateID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_State_Language_TB_State");
        });

        if (DBType == "POSTGRESQL")
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
                property.SetColumnType("timestamp without time zone");
        }

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
