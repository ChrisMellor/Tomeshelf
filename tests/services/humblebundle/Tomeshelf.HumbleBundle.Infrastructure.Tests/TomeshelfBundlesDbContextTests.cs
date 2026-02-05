using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.HumbleBundle.Domain.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Infrastructure.Tests;

public class TomeshelfBundlesDbContextTests
{
    [Fact]
    public void BundleEntity_IsConfiguredWithExpectedMetadata()
    {
        var options = new DbContextOptionsBuilder<TomeshelfBundlesDbContext>().UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=HumbleBundleModelTest;Trusted_Connection=True;")
                                                                              .Options;

        using var context = new TomeshelfBundlesDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Bundle));

        entityType.Should()
                  .NotBeNull();
        entityType!.GetTableName()
                   .Should()
                   .Be("Bundles");

        var machineNameProperty = entityType.FindProperty(nameof(Bundle.MachineName));
        machineNameProperty.Should()
                           .NotBeNull();
        machineNameProperty!.GetMaxLength()
                            .Should()
                            .Be(200);
        machineNameProperty.IsNullable
                           .Should()
                           .BeFalse();

        entityType.GetIndexes()
                  .Single(index => index.Properties.Contains(machineNameProperty))
                  .IsUnique
                  .Should()
                  .BeTrue();

        entityType.FindProperty(nameof(Bundle.ShortDescription))
                 ?.GetMaxLength()
                  .Should()
                  .Be(1024);

        entityType.FindProperty(nameof(Bundle.FirstSeenUtc))
                 ?.GetColumnType()
                  .Should()
                  .Be("datetimeoffset(0)");
        entityType.FindProperty(nameof(Bundle.LastSeenUtc))
                 ?.GetColumnType()
                  .Should()
                  .Be("datetimeoffset(0)");
        entityType.FindProperty(nameof(Bundle.LastUpdatedUtc))
                 ?.GetColumnType()
                  .Should()
                  .Be("datetimeoffset(0)");
        entityType.FindProperty(nameof(Bundle.StartsAt))
                 ?.GetColumnType()
                  .Should()
                  .Be("datetimeoffset(0)");
        entityType.FindProperty(nameof(Bundle.EndsAt))
                 ?.GetColumnType()
                  .Should()
                  .Be("datetimeoffset(0)");
    }
}