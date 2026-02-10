using System;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.Fitbit.Domain;

namespace Tomeshelf.Fitbit.Infrastructure;

/// <summary>
///     EF Core database context for persisted Fitbit snapshots.
/// </summary>
/// <param name="options">DbContext options configured by dependency injection.</param>
public sealed class TomeshelfFitbitDbContext(DbContextOptions<TomeshelfFitbitDbContext> options) : DbContext(options)
{
    public DbSet<FitbitDailySnapshot> DailySnapshots => Set<FitbitDailySnapshot>();

    /// <summary>
    ///     Ons the model creating.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
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

        snapshot.Property(x => x.BodyFatPercentage);
        snapshot.Property(x => x.LeanMassKg);
        snapshot.Property(x => x.CarbsGrams);
        snapshot.Property(x => x.FatGrams);
        snapshot.Property(x => x.FiberGrams);
        snapshot.Property(x => x.ProteinGrams);
        snapshot.Property(x => x.SodiumMilligrams);
        snapshot.Property(x => x.SleepDeepMinutes);
        snapshot.Property(x => x.SleepLightMinutes);
        snapshot.Property(x => x.SleepRemMinutes);
        snapshot.Property(x => x.SleepWakeMinutes);

        snapshot.HasIndex(x => x.GeneratedUtc);
    }
}