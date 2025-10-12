using AdControl.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AdControl.Core.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts)
    {
    }

    public DbSet<Screen> Screens => Set<Screen>();
    public DbSet<Config> Configs => Set<Config>();
    public DbSet<ConfigItem> ConfigItems => Set<ConfigItem>();
    public DbSet<ScreenConfig> ScreenConfigs => Set<ScreenConfig>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        model.Entity<Screen>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200);
            b.Property(x => x.Location).HasMaxLength(500);
            b.Property(x => x.Resolution).HasMaxLength(64);
            b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        });

        model.Entity<Config>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            b.HasMany(x => x.Items).WithOne(i => i.Config).HasForeignKey(i => i.ConfigId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<ConfigItem>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).HasMaxLength(50);
            b.Property(x => x.UrlOrData).HasMaxLength(2000);
        });

        model.Entity<ScreenConfig>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(s => s.Screen).WithMany(s => s.ScreenConfigs).HasForeignKey(s => s.ScreenId);
            b.HasOne(s => s.Config).WithMany().HasForeignKey(s => s.ConfigId);
        });
    }
}