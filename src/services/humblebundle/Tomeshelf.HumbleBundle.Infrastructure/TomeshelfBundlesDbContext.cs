using Tomeshelf.HumbleBundle.Domain.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Infrastructure;

/// <summary>
///     EF Core database context for Tomeshelf's bundle domain entities.
/// </summary>
/// <param name="options">DbContext options configured by DI.</param>
public class TomeshelfBundlesDbContext(DbContextOptions<TomeshelfBundlesDbContext> options) : DbContext(options)
{
    /// <summary>
    ///     Catalog of Humble Bundle listings captured from the public site.
    /// </summary>
    public DbSet<Bundle> Bundles => Set<Bundle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Bundle>(entity =>
        {
            entity.ToTable("Bundles");
            entity.HasKey(b => b.Id);

            entity.HasIndex(b => b.MachineName)
                  .IsUnique();
            entity.Property(b => b.MachineName)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.Property(b => b.Category)
                  .HasMaxLength(100);
            entity.Property(b => b.Stamp)
                  .HasMaxLength(50);
            entity.Property(b => b.Title)
                  .HasMaxLength(512);
            entity.Property(b => b.ShortName)
                  .HasMaxLength(256);
            entity.Property(b => b.Url)
                  .HasMaxLength(512);
            entity.Property(b => b.TileImageUrl)
                  .HasMaxLength(512);
            entity.Property(b => b.HeroImageUrl)
                  .HasMaxLength(512);
            entity.Property(b => b.TileLogoUrl)
                  .HasMaxLength(512);
            entity.Property(b => b.ShortDescription)
                  .HasMaxLength(1024);

            entity.Property(b => b.FirstSeenUtc)
                  .HasColumnType("datetimeoffset(0)");
            entity.Property(b => b.LastSeenUtc)
                  .HasColumnType("datetimeoffset(0)");
            entity.Property(b => b.LastUpdatedUtc)
                  .HasColumnType("datetimeoffset(0)");
            entity.Property(b => b.StartsAt)
                  .HasColumnType("datetimeoffset(0)");
            entity.Property(b => b.EndsAt)
                  .HasColumnType("datetimeoffset(0)");
        });
    }
}