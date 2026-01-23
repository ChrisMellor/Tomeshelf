using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.Domain.Shared.Entities.SHiFT;

namespace Tomeshelf.Infrastructure.Shared.Persistence;

public sealed class TomeshelfShiftDbContext : DbContext, IDataProtectionKeyContext
{
    public TomeshelfShiftDbContext(DbContextOptions<TomeshelfShiftDbContext> options) : base(options) { }

    public DbSet<SettingsEntity> ShiftSettings => Set<SettingsEntity>();

    // Data Protection keys table
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SettingsEntity>(b =>
        {
            b.ToTable("ShiftSettings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Email)
             .HasMaxLength(256);
            b.Property(x => x.DefaultService)
             .HasMaxLength(32);
            b.Property(x => x.EncryptedPassword)
             .HasMaxLength(4000);
        });

        base.OnModelCreating(modelBuilder);
    }
}