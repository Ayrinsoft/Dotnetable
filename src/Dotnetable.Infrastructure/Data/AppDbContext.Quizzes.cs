using Dotnetable.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Data;

public partial class AppDbContext
{
    public virtual DbSet<QuizCategory> QuizCategories { get; set; }

    public virtual DbSet<QuizCategoryTranslation> QuizCategoryTranslations { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizTranslation> QuizTranslations { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<QuizQuestionImage> QuizQuestionImages { get; set; }

    public virtual DbSet<QuizOption> QuizOptions { get; set; }

    public virtual DbSet<QuizScoreBand> QuizScoreBands { get; set; }

    public virtual DbSet<QuizScoreBandTranslation> QuizScoreBandTranslations { get; set; }

    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }

    public virtual DbSet<QuizAttemptQuestion> QuizAttemptQuestions { get; set; }

    public virtual DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }

    public virtual DbSet<QuizAttemptAnswerOption> QuizAttemptAnswerOptions { get; set; }

    private static void ConfigureQuizzes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuizCategory>(entity =>
        {
            entity.HasIndex(e => new { e.WebsiteID, e.Slug }, "UQ_QuizCategories_WebsiteID_Slug").IsUnique();
            entity.HasIndex(e => e.ParentCategoryID, "IX_QuizCategories_ParentCategoryID");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(160);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.HasOne(d => d.Website).WithMany()
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.ParentCategory).WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentCategoryID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuizCategoryTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.QuizCategoryID, e.LanguageCode }, "UQ_QuizCategoryTranslations_Category_Language").IsUnique();
            entity.Property(e => e.LanguageCode).HasMaxLength(2).IsUnicode(false).IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.HasOne(d => d.QuizCategory).WithMany(p => p.Translations)
                .HasForeignKey(d => d.QuizCategoryID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasIndex(e => new { e.WebsiteID, e.Slug }, "UQ_Quizzes_WebsiteID_Slug").IsUnique();
            entity.HasIndex(e => e.QuizCategoryID, "IX_Quizzes_QuizCategoryID");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(160);
            entity.Property(e => e.Summary).HasMaxLength(2000);
            entity.Property(e => e.PrivateReviewFee).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CurrencyCode).HasMaxLength(3).IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.HasOne(d => d.Website).WithMany()
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.QuizCategory).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.QuizCategoryID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuizTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.QuizID, e.LanguageCode }, "UQ_QuizTranslations_Quiz_Language").IsUnique();
            entity.Property(e => e.LanguageCode).HasMaxLength(2).IsUnicode(false).IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Summary).HasMaxLength(2000);
            entity.HasOne(d => d.Quiz).WithMany(p => p.Translations)
                .HasForeignKey(d => d.QuizID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasIndex(e => e.QuizID, "IX_QuizQuestions_QuizID");
            entity.Property(e => e.Points).HasColumnType("decimal(9, 2)");
            entity.HasOne(d => d.Quiz).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuizID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizQuestionImage>(entity =>
        {
            entity.HasIndex(e => e.QuizQuestionID, "IX_QuizQuestionImages_QuizQuestionID");
            entity.HasIndex(e => e.FileRecordID, "IX_QuizQuestionImages_FileRecordID");
            entity.HasOne(d => d.QuizQuestion).WithMany(p => p.Images)
                .HasForeignKey(d => d.QuizQuestionID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.FileRecord).WithMany()
                .HasForeignKey(d => d.FileRecordID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuizOption>(entity =>
        {
            entity.HasIndex(e => e.QuizQuestionID, "IX_QuizOptions_QuizQuestionID");
            entity.Property(e => e.Text).HasMaxLength(1000);
            entity.HasOne(d => d.QuizQuestion).WithMany(p => p.Options)
                .HasForeignKey(d => d.QuizQuestionID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizScoreBand>(entity =>
        {
            entity.HasIndex(e => e.QuizID, "IX_QuizScoreBands_QuizID");
            entity.Property(e => e.MinPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MaxPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.HasOne(d => d.Quiz).WithMany(p => p.ScoreBands)
                .HasForeignKey(d => d.QuizID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizScoreBandTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.QuizScoreBandID, e.LanguageCode }, "UQ_QuizScoreBandTranslations_Band_Language").IsUnique();
            entity.Property(e => e.LanguageCode).HasMaxLength(2).IsUnicode(false).IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.HasOne(d => d.QuizScoreBand).WithMany(p => p.Translations)
                .HasForeignKey(d => d.QuizScoreBandID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasIndex(e => e.PublicToken, "UQ_QuizAttempts_PublicToken").IsUnique();
            entity.HasIndex(e => e.QuizID, "IX_QuizAttempts_QuizID");
            entity.HasIndex(e => e.GatewayAuthority, "IX_QuizAttempts_GatewayAuthority");
            entity.Property(e => e.Score).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.MaxScore).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Percent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PrivateReviewFee).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.BandTitle).HasMaxLength(200);
            entity.Property(e => e.ContactName).HasMaxLength(120);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.CurrencyCode).HasMaxLength(3).IsUnicode(false);
            entity.Property(e => e.GatewayAuthority).HasMaxLength(200);
            entity.Property(e => e.GatewayRef).HasMaxLength(100);
            entity.Property(e => e.LanguageCode).HasMaxLength(2).IsUnicode(false).IsFixedLength();
            entity.Property(e => e.StartedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.SubmittedAt).HasColumnType("datetime");
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.PrivateAnswerAt).HasColumnType("datetime");
            entity.HasOne(d => d.Quiz).WithMany(p => p.Attempts)
                .HasForeignKey(d => d.QuizID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Website>().WithMany()
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.WebsiteClient).WithMany()
                .HasForeignKey(d => d.WebsiteClientID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Payment).WithMany()
                .HasForeignKey(d => d.PaymentID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.PaymentGateway).WithMany()
                .HasForeignKey(d => d.PaymentGatewayID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuizAttemptQuestion>(entity =>
        {
            entity.HasKey(e => new { e.QuizAttemptID, e.QuizQuestionID });
            entity.HasOne(d => d.QuizAttempt).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuizAttemptID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.QuizQuestion).WithMany()
                .HasForeignKey(d => d.QuizQuestionID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuizAttemptAnswer>(entity =>
        {
            entity.HasIndex(e => e.QuizAttemptID, "IX_QuizAttemptAnswers_QuizAttemptID");
            entity.Property(e => e.PointsAwarded).HasColumnType("decimal(9, 2)");
            entity.HasOne(d => d.QuizAttempt).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuizAttemptID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.QuizQuestion).WithMany()
                .HasForeignKey(d => d.QuizQuestionID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuizAttemptAnswerOption>(entity =>
        {
            entity.HasKey(e => new { e.QuizAttemptAnswerID, e.QuizOptionID });
            entity.HasOne(d => d.QuizAttemptAnswer).WithMany(p => p.SelectedOptions)
                .HasForeignKey(d => d.QuizAttemptAnswerID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.QuizOption).WithMany()
                .HasForeignKey(d => d.QuizOptionID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasIndex(e => e.QuizID, "IX_MenuItems_QuizID");
            entity.HasIndex(e => e.QuizCategoryID, "IX_MenuItems_QuizCategoryID");
            entity.HasOne(d => d.Quiz).WithMany()
                .HasForeignKey(d => d.QuizID)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(d => d.QuizCategory).WithMany()
                .HasForeignKey(d => d.QuizCategoryID)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
