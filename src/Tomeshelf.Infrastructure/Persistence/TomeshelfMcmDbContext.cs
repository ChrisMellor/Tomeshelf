using Microsoft.EntityFrameworkCore;
using Tomeshelf.Domain.Entities.Mcm;

namespace Tomeshelf.Infrastructure.Persistence;

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
        });

        modelBuilder.Entity<GuestEntity>(g =>
        {
            g.ToTable("Guests");

            g.Property(x => x.AddedAt)
             .IsRequired()
             .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            g.Property(x => x.RemovedAt);
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
