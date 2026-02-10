using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.SHiFT.Domain.Entities;
using Tomeshelf.SHiFT.Infrastructure.Persistence;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Persistence.TomeshelfShiftDbContextTests;

public class OnModelCreating
{
    /// <summary>
    ///     Settings entity is configured with expected metadata.
    /// </summary>
    [Fact]
    public void SettingsEntity_IsConfiguredWithExpectedMetadata()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TomeshelfShiftDbContext>().UseInMemoryDatabase($"ShiftSettingsTest-{Guid.NewGuid():N}")
                                                                            .Options;

        using var context = new TomeshelfShiftDbContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(SettingsEntity));

        // Assert
        entityType.ShouldNotBeNull();
        entityType!.GetTableName()
                   .ShouldBe("ShiftSettings");

        var emailProperty = entityType.FindProperty(nameof(SettingsEntity.Email));
        emailProperty.ShouldNotBeNull();
        emailProperty!.GetMaxLength()
                      .ShouldBe(256);

        var serviceProperty = entityType.FindProperty(nameof(SettingsEntity.DefaultService));
        serviceProperty.ShouldNotBeNull();
        serviceProperty!.GetMaxLength()
                        .ShouldBe(32);

        var passwordProperty = entityType.FindProperty(nameof(SettingsEntity.EncryptedPassword));
        passwordProperty.ShouldNotBeNull();
        passwordProperty!.GetMaxLength()
                         .ShouldBe(4000);
    }
}