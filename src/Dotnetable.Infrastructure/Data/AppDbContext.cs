using Dotnetable.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Website> Websites => Set<Website>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Translation> Translations => Set<Translation>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<WebsiteLanguage> WebsiteLanguages => Set<WebsiteLanguage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WebsiteLanguage>()
            .HasKey(wl => new { wl.WebsiteId, wl.LanguageId });

        modelBuilder.Entity<Translation>()
            .HasIndex(t => new { t.LanguageId, t.WebsiteId, t.Key })
            .IsUnique();

        modelBuilder.Entity<ApiKey>()
            .HasIndex(a => a.Key)
            .IsUnique();

        modelBuilder.Entity<Website>()
            .HasIndex(w => w.Domain)
            .IsUnique();

        modelBuilder.Entity<Post>()
            .HasIndex(p => new { p.WebsiteId, p.Slug, p.LanguageId })
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.WebsiteId, p.Slug, p.LanguageId })
            .IsUnique();

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Product>()
            .Property(p => p.SalePrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
