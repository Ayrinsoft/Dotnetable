using Dotnetable.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Data;

public partial class AppDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookingProfile>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "UQ_BookingProfiles_WebsiteID").IsUnique();
            entity.Property(e => e.TimeZoneId).HasMaxLength(64);
            entity.Property(e => e.BookThrough).HasColumnType("date");
            entity.Property(e => e.RetentionDays).HasDefaultValue(180);
            entity.Property(e => e.RequirePayment).HasDefaultValue(true);
            entity.Property(e => e.AllowOnlinePayment).HasDefaultValue(true);
            entity.Property(e => e.AllowOfflinePayment).HasDefaultValue(true);
            entity.Property(e => e.HoldMinutes).HasDefaultValue(30);
            entity.Property(e => e.LeadMinutes).HasDefaultValue(60);
            entity.Property(e => e.DayStartMinutes).HasDefaultValue(540);
            entity.Property(e => e.DayEndMinutes).HasDefaultValue(1020);
            entity.Property(e => e.WorkDays).HasDefaultValue((byte)31);
            entity.HasOne(d => d.Website).WithMany()
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingResource>(entity =>
        {
            entity.HasIndex(e => e.WebsiteID, "IX_BookingResources_WebsiteID");
            entity.Property(e => e.Name).HasMaxLength(120);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.DayStartMinutes).HasDefaultValue(540);
            entity.Property(e => e.DayEndMinutes).HasDefaultValue(1020);
            entity.Property(e => e.WorkDays).HasDefaultValue((byte)31);
            entity.HasOne(d => d.Website).WithMany()
                .HasForeignKey(d => d.WebsiteID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingOffering>(entity =>
        {
            entity.HasIndex(e => e.BookingResourceID, "IX_BookingOfferings_BookingResourceID");
            entity.HasIndex(e => e.WebsiteID, "IX_BookingOfferings_WebsiteID");
            entity.Property(e => e.Name).HasMaxLength(160);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FullPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(d => d.Resource).WithMany(p => p.Offerings)
                .HasForeignKey(d => d.BookingResourceID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingClosure>(entity =>
        {
            entity.HasIndex(e => new { e.WebsiteID, e.Date }, "IX_BookingClosures_WebsiteID_Date");
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Note).HasMaxLength(200);
            entity.HasOne(d => d.Resource).WithMany()
                .HasForeignKey(d => d.BookingResourceID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingAppointment>(entity =>
        {
            entity.HasIndex(e => new { e.BookingResourceID, e.StartsAtUtc }, "IX_BookingAppointments_Resource_Start");
            entity.HasIndex(e => e.WebsiteID, "IX_BookingAppointments_WebsiteID");
            entity.HasIndex(e => e.OrderID, "IX_BookingAppointments_OrderID");
            entity.Property(e => e.CustomerName).HasMaxLength(120);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.StartsAtUtc).HasColumnType("datetime");
            entity.Property(e => e.EndsAtUtc).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.HasOne(d => d.Resource).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.BookingResourceID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Offering).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.BookingOfferingID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Order).WithMany()
                .HasForeignKey(d => d.OrderID)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
