using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Infrastructure.Persistence;

public sealed class TomeshelfShiftDbContext : DbContext, IDataProtectionKeyContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TomeshelfShiftDbContext" /> class.
    /// </summary>
    /// <param name="options">The options.</param>
    public TomeshelfShiftDbContext(DbContextOptions<TomeshelfShiftDbContext> options) : base(options) { }

    public DbSet<SettingsEntity> ShiftSettings => Set<SettingsEntity>();

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = default!;

    /// <summary>
    ///     Ons the model creating.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
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