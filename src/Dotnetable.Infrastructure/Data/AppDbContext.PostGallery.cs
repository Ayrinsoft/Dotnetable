using Dotnetable.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Data;

public partial class AppDbContext
{
    private static void ConfigurePostGallery(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostImage>(entity =>
        {
            entity.HasIndex(e => e.PostID, "IX_PostImages_PostID");
            entity.HasIndex(e => e.FileRecordID, "IX_PostImages_FileRecordID");
            entity.HasIndex(e => new { e.PostID, e.FileRecordID }, "UQ_PostImages_Post_File").IsUnique();

            entity.HasOne(d => d.Post).WithMany(p => p.PostImages)
                .HasForeignKey(d => d.PostID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostImages_Posts");

            entity.HasOne(d => d.FileRecord).WithMany(p => p.PostImages)
                .HasForeignKey(d => d.FileRecordID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostImages_FileRecords");
        });

        modelBuilder.Entity<PostInstagramPublication>(entity =>
        {
            entity.HasKey(e => e.PostID);
            entity.Property(e => e.IgMediaId).HasMaxLength(64);
            entity.Property(e => e.Permalink).HasMaxLength(300);
            entity.Property(e => e.PublishedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Post).WithOne(p => p.PostInstagramPublication)
                .HasForeignKey<PostInstagramPublication>(d => d.PostID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostInstagramPublications_Posts");
        });

        modelBuilder.Entity<WebsiteInstagramAccount>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_WebsiteInstagramAccounts_WebsiteID");
            entity.Property(e => e.Title).HasMaxLength(80);
            entity.Property(e => e.LanguageCode).HasMaxLength(10);
            entity.Property(e => e.IgUserId).HasMaxLength(40);
            entity.Property(e => e.PageId).HasMaxLength(40);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteInstagramAccounts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteInstagramAccounts_Websites");
        });

        modelBuilder.Entity<PostTelegramPublication>(entity =>
        {
            entity.HasKey(e => e.PostID);
            entity.Property(e => e.MessageId).HasMaxLength(40);
            entity.Property(e => e.PublishedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Post).WithOne(p => p.PostTelegramPublication)
                .HasForeignKey<PostTelegramPublication>(d => d.PostID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostTelegramPublications_Posts");
        });

        modelBuilder.Entity<PostXPublication>(entity =>
        {
            entity.HasKey(e => e.PostID);
            entity.Property(e => e.TweetId).HasMaxLength(40);
            entity.Property(e => e.PublishedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Post).WithOne(p => p.PostXPublication)
                .HasForeignKey<PostXPublication>(d => d.PostID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostXPublications_Posts");
        });

        modelBuilder.Entity<WebsiteTelegramAccount>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_WebsiteTelegramAccounts_WebsiteID");
            entity.Property(e => e.Title).HasMaxLength(80);
            entity.Property(e => e.LanguageCode).HasMaxLength(10);
            entity.Property(e => e.ChatId).HasMaxLength(80);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteTelegramAccounts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteTelegramAccounts_Websites");
        });

        modelBuilder.Entity<ProductInstagramPublication>(entity =>
        {
            entity.HasKey(e => e.ProductID);
            entity.Property(e => e.IgMediaId).HasMaxLength(64);
            entity.Property(e => e.Permalink).HasMaxLength(300);
            entity.Property(e => e.PublishedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Product).WithOne(p => p.ProductInstagramPublication)
                .HasForeignKey<ProductInstagramPublication>(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductInstagramPublications_Products");
        });

        modelBuilder.Entity<ProductTelegramPublication>(entity =>
        {
            entity.HasKey(e => e.ProductID);
            entity.Property(e => e.MessageId).HasMaxLength(40);
            entity.Property(e => e.PublishedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Product).WithOne(p => p.ProductTelegramPublication)
                .HasForeignKey<ProductTelegramPublication>(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductTelegramPublications_Products");
        });

        modelBuilder.Entity<ProductXPublication>(entity =>
        {
            entity.HasKey(e => e.ProductID);
            entity.Property(e => e.TweetId).HasMaxLength(40);
            entity.Property(e => e.PublishedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Product).WithOne(p => p.ProductXPublication)
                .HasForeignKey<ProductXPublication>(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductXPublications_Products");
        });

        modelBuilder.Entity<WebsiteXAccount>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_WebsiteXAccounts_WebsiteID");
            entity.Property(e => e.Title).HasMaxLength(80);
            entity.Property(e => e.LanguageCode).HasMaxLength(10);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Website).WithMany(p => p.WebsiteXAccounts)
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteXAccounts_Websites");
        });
    }
}
