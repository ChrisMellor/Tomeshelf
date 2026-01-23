using Microsoft.EntityFrameworkCore.Migrations;

namespace Tomeshelf.Infrastructure.Persistence.Migrations.Fitbit;

/// <inheritdoc />
public partial class ExtendFitbitDashboardMetrics : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<double>("BodyFatPercentage", "FitbitDailySnapshots", "float", nullable: true);

        migrationBuilder.AddColumn<double>("CarbsGrams", "FitbitDailySnapshots", "float", nullable: true);

        migrationBuilder.AddColumn<double>("FatGrams", "FitbitDailySnapshots", "float", nullable: true);

        migrationBuilder.AddColumn<double>("FiberGrams", "FitbitDailySnapshots", "float", nullable: true);

        migrationBuilder.AddColumn<double>("LeanMassKg", "FitbitDailySnapshots", "float", nullable: true);

        migrationBuilder.AddColumn<double>("ProteinGrams", "FitbitDailySnapshots", "float", nullable: true);

        migrationBuilder.AddColumn<int>("SleepDeepMinutes", "FitbitDailySnapshots", "int", nullable: true);

        migrationBuilder.AddColumn<int>("SleepLightMinutes", "FitbitDailySnapshots", "int", nullable: true);

        migrationBuilder.AddColumn<int>("SleepRemMinutes", "FitbitDailySnapshots", "int", nullable: true);

        migrationBuilder.AddColumn<int>("SleepWakeMinutes", "FitbitDailySnapshots", "int", nullable: true);

        migrationBuilder.AddColumn<double>("SodiumMilligrams", "FitbitDailySnapshots", "float", nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("BodyFatPercentage", "FitbitDailySnapshots");

        migrationBuilder.DropColumn("CarbsGrams", "FitbitDailySnapshots");

        migrationBuilder.DropColumn("FatGrams", "FitbitDailySnapshots");

        migrationBuilder.DropColumn("FiberGrams", "FitbitDailySnapshots");

        migrationBuilder.DropColumn("LeanMassKg", "FitbitDailySnapshots");

        migrationBuilder.DropColumn("ProteinGrams", "FitbitDailySnapshots");

        migrationBuilder.DropColumn("SleepDeepMinutes", "FitbitDailySnapshots");

        migrationBuilder.DropColumn("SleepLightMinutes", "FitbitDailySnapshots");

        migrationBuilder.DropColumn("SleepRemMinutes", "FitbitDailySnapshots");

        migrationBuilder.DropColumn("SleepWakeMinutes", "FitbitDailySnapshots");

        migrationBuilder.DropColumn("SodiumMilligrams", "FitbitDailySnapshots");
    }
}