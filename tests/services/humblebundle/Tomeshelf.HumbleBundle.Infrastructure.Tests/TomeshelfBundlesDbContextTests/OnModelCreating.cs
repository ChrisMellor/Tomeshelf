using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.HumbleBundle.Domain.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Infrastructure.Tests.TomeshelfBundlesDbContextTests;

public class OnModelCreating
{
    [Fact]
    public void BundleEntity_IsConfiguredWithExpectedMetadata()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TomeshelfBundlesDbContext>().UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=HumbleBundleModelTest;Trusted_Connection=True;")
                                                                              .Options;

        using var context = new TomeshelfBundlesDbContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(Bundle));

        // Assert
        entityType.ShouldNotBeNull();
        entityType!.GetTableName()
                   .ShouldBe("Bundles");

        var machineNameProperty = entityType.FindProperty(nameof(Bundle.MachineName));
        machineNameProperty.ShouldNotBeNull();
        machineNameProperty!.GetMaxLength()
                            .ShouldBe(200);
        machineNameProperty.IsNullable.ShouldBeFalse();

        var machineNameIndex = entityType.GetIndexes()
                                         .Single(index => index.Properties.Contains(machineNameProperty));
        machineNameIndex.IsUnique.ShouldBeTrue();

        var shortDescriptionProperty = entityType.FindProperty(nameof(Bundle.ShortDescription));
        shortDescriptionProperty.ShouldNotBeNull();
        shortDescriptionProperty!.GetMaxLength()
                                 .ShouldBe(1024);

        entityType.FindProperty(nameof(Bundle.FirstSeenUtc))
                 ?.GetColumnType()
                  .ShouldBe("datetimeoffset(0)");
        entityType.FindProperty(nameof(Bundle.LastSeenUtc))
                 ?.GetColumnType()
                  .ShouldBe("datetimeoffset(0)");
        entityType.FindProperty(nameof(Bundle.LastUpdatedUtc))
                 ?.GetColumnType()
                  .ShouldBe("datetimeoffset(0)");
        entityType.FindProperty(nameof(Bundle.StartsAt))
                 ?.GetColumnType()
                  .ShouldBe("datetimeoffset(0)");
        entityType.FindProperty(nameof(Bundle.EndsAt))
                 ?.GetColumnType()
                  .ShouldBe("datetimeoffset(0)");
    }
}