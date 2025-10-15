using System;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.Domain.Entities.Fitness;

namespace Tomeshelf.Infrastructure.Persistence;

/// <summary>
///     EF Core database context for persisted Fitbit snapshots.
/// </summary>
/// <param name="options">DbContext options configured by dependency injection.</param>
public sealed class TomeshelfFitbitDbContext(DbContextOptions<TomeshelfFitbitDbContext> options) : DbContext(options)
{
    public DbSet<FitbitDailySnapshot> DailySnapshots => Set<FitbitDailySnapshot>();

    public DbSet<FitbitCredential> FitbitCredentials => Set<FitbitCredential>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var snapshot = modelBuilder.Entity<FitbitDailySnapshot>();

        snapshot.ToTable("FitbitDailySnapshots");
        snapshot.HasKey(x => x.Date);

        snapshot.Property(x => x.Date)
                .HasColumnType("date")
                .HasConversion(value => value.ToDateTime(TimeOnly.MinValue), value => DateOnly.FromDateTime(value));

        snapshot.Property(x => x.GeneratedUtc)
                .IsRequired();

        snapshot.Property(x => x.Bedtime)
                .HasMaxLength(10);

        snapshot.Property(x => x.WakeTime)
                .HasMaxLength(10);

        snapshot.HasIndex(x => x.GeneratedUtc);
        var credential = modelBuilder.Entity<FitbitCredential>();
        credential.ToTable("FitbitCredentials");
        credential.HasKey(x => x.Id);
        credential.Property(x => x.Id)
                  .ValueGeneratedNever();
        credential.Property(x => x.AccessToken)
                  .HasMaxLength(2048);
        credential.Property(x => x.RefreshToken)
                  .HasMaxLength(2048);
        credential.Property(x => x.ExpiresAtUtc);
    }
}

