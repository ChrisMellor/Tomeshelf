using Microsoft.EntityFrameworkCore;
using Tomeshelf.Domain.Entities.Mcm;

namespace Tomeshelf.Infrastructure.Persistence;

public sealed class TomeshelfMcmDbContext : DbContext
{
    public TomeshelfMcmDbContext(DbContextOptions<TomeshelfMcmDbContext> options) : base(options) { }

    public DbSet<EventConfigEntity> EventConfigs => Set<EventConfigEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventConfigEntity>(e =>
        {
            e.ToTable("EventConfigs");

            e.HasKey(x => x.Id);

            e.Property(x => x.Name)
             .HasMaxLength(30)
             .IsRequired();

            e.Property(x => x.UpdatedAt)
             .IsRequired();
        });
    }
}