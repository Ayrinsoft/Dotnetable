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
            entity.HasIndex(e => e.WebsiteID, "UQ_WebsiteInstagramAccounts_WebsiteID").IsUnique();
            entity.Property(e => e.IgUserId).HasMaxLength(40);
            entity.Property(e => e.PageId).HasMaxLength(40);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Website).WithOne(p => p.WebsiteInstagramAccount)
                .HasForeignKey<WebsiteInstagramAccount>(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WebsiteInstagramAccounts_Websites");
        });
    }
}
