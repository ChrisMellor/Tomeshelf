using Microsoft.EntityFrameworkCore;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Infrastructure;

public sealed class TomeshelfMcmDbContext : DbContext
{
    public TomeshelfMcmDbContext(DbContextOptions<TomeshelfMcmDbContext> options) : base(options) { }

    public DbSet<EventEntity> Events => Set<EventEntity>();

    public DbSet<GuestEntity> Guests => Set<GuestEntity>();

    public DbSet<GuestInfoEntity> Information => Set<GuestInfoEntity>();

    public DbSet<GuestSocial> Social => Set<GuestSocial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventEntity>(e =>
        {
            e.ToTable("Events");

            e.HasKey(x => x.Id);

            e.Property(x => x.Name)
             .HasMaxLength(30)
             .IsRequired();

            e.Property(x => x.UpdatedAt)
             .IsRequired();

            e.HasMany(e => e.Guests)
             .WithOne(g => g.Event)
             .HasForeignKey(g => g.EventId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GuestEntity>(g =>
        {
            g.ToTable("Guests");

            g.Property(x => x.AddedAt)
             .IsRequired()
             .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            g.Property(x => x.RemovedAt);

            g.HasOne(g => g.Information)
             .WithOne(gi => gi.Guest)
             .HasForeignKey<GuestInfoEntity>(gi => gi.GuestId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GuestInfoEntity>(g =>
        {
            g.ToTable("GuestInformation");
        });

        modelBuilder.Entity<GuestSocial>(s =>
        {
            s.ToTable("GuestSocials");
        });
    }
}