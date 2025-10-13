using Microsoft.EntityFrameworkCore;
using Tomeshelf.Domain.Entities.ComicCon;

namespace Tomeshelf.Infrastructure.Persistence;

/// <summary>
///     EF Core database context for Tomeshelf's ComicCon domain entities.
/// </summary>
/// <param name="options">DbContext options configured by DI.</param>
public class TomeshelfComicConDbContext(DbContextOptions<TomeshelfComicConDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();

    public DbSet<Person> People => Set<Person>();

    public DbSet<PersonImage> PersonImages => Set<PersonImage>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<PersonCategory> PersonCategories => Set<PersonCategory>();

    public DbSet<EventAppearance> EventAppearances => Set<EventAppearance>();

    public DbSet<Schedule> Schedules => Set<Schedule>();

    public DbSet<VenueLocation> VenueLocations => Set<VenueLocation>();

    /// <summary>
    ///     Configures EF Core model conventions, relationships and constraints for domain entities.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure mappings.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(e =>
        {
            e.HasIndex(x => x.ExternalId)
             .IsUnique();
            e.HasIndex(x => x.Slug)
             .IsUnique();
            e.Property(x => x.Name)
             .HasMaxLength(300)
             .IsRequired();
            e.Property(x => x.Slug)
             .HasMaxLength(200)
             .IsRequired();
        });

        modelBuilder.Entity<Person>(p =>
        {
            p.HasIndex(x => x.ExternalId)
             .IsUnique();
            p.Property(x => x.FirstName)
             .HasMaxLength(150);
            p.Property(x => x.LastName)
             .HasMaxLength(150);
            p.Property(x => x.AltName)
             .HasMaxLength(200);
            p.Property(x => x.KnownFor)
             .HasMaxLength(500);

            p.Property(x => x.ProfileUrl)
             .HasMaxLength(1000);
            p.Property(x => x.VideoLink)
             .HasMaxLength(1000);
            p.Property(x => x.Twitter)
             .HasMaxLength(300);
            p.Property(x => x.Facebook)
             .HasMaxLength(300);
            p.Property(x => x.Instagram)
             .HasMaxLength(300);
            p.Property(x => x.YouTube)
             .HasMaxLength(300);
            p.Property(x => x.Twitch)
             .HasMaxLength(300);
            p.Property(x => x.Snapchat)
             .HasMaxLength(300);
            p.Property(x => x.DeviantArt)
             .HasMaxLength(300);
            p.Property(x => x.Tumblr)
             .HasMaxLength(300);
        });

        modelBuilder.Entity<PersonImage>(i =>
        {
            i.Property(x => x.Big)
             .HasMaxLength(1000);
            i.Property(x => x.Med)
             .HasMaxLength(1000);
            i.Property(x => x.Small)
             .HasMaxLength(1000);
            i.Property(x => x.Thumb)
             .HasMaxLength(1000);
            i.HasOne(x => x.Person)
             .WithMany(p => p.Images)
             .HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<Category>(c =>
        {
            c.HasIndex(x => x.ExternalId)
             .IsUnique();
            c.Property(x => x.Name)
             .HasMaxLength(200)
             .IsRequired();
        });

        modelBuilder.Entity<PersonCategory>(pc =>
        {
            pc.HasKey(x => new
            {
                    x.PersonId,
                    x.CategoryId
            });
            pc.HasOne(x => x.Person)
              .WithMany(p => p.Categories)
              .HasForeignKey(x => x.PersonId);
            pc.HasOne(x => x.Category)
              .WithMany(c => c.PersonLinks)
              .HasForeignKey(x => x.CategoryId);
        });

        modelBuilder.Entity<EventAppearance>(a =>
        {
            a.HasOne(x => x.Event)
             .WithMany(e => e.Appearances)
             .HasForeignKey(x => x.EventId);
            a.HasOne(x => x.Person)
             .WithMany(p => p.Appearances)
             .HasForeignKey(x => x.PersonId);
            a.HasIndex(x => new
              {
                      x.EventId,
                      x.PersonId
              })
             .IsUnique();
            a.Property(x => x.DaysAtShow)
             .HasMaxLength(50);
            a.Property(x => x.BoothNumber)
             .HasMaxLength(200);
            a.Property(x => x.AutographAmount)
             .HasColumnType("decimal(10,2)");
            a.Property(x => x.PhotoOpAmount)
             .HasColumnType("decimal(10,2)");
            a.Property(x => x.PhotoOpTableAmount)
             .HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<VenueLocation>(v =>
        {
            v.HasIndex(x => x.ExternalId)
             .IsUnique();
            v.Property(x => x.Name)
             .HasMaxLength(300)
             .IsRequired();
        });

        modelBuilder.Entity<Schedule>(s =>
        {
            s.HasIndex(x => new
              {
                      x.EventAppearanceId,
                      x.ExternalId
              })
             .IsUnique();
            s.Property(x => x.Title)
             .HasMaxLength(300)
             .IsRequired();
            s.Property(x => x.Location)
             .HasMaxLength(300);
            s.HasOne(x => x.EventAppearance)
             .WithMany(a => a.Schedules)
             .HasForeignKey(x => x.EventAppearanceId);
            s.HasOne(x => x.VenueLocation)
             .WithMany(v => v.Schedules)
             .HasForeignKey(x => x.VenueLocationId);
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TomeshelfComicConDbContext).Assembly);
    }
}