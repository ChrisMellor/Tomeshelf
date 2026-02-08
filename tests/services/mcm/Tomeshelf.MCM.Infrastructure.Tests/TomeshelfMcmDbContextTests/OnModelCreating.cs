using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Infrastructure.Tests.TomeshelfMcmDbContextTests;

public class OnModelCreating
{
    [Fact]
    public void Entities_AreConfiguredWithExpectedMetadata()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TomeshelfMcmDbContext>().UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=McmModelTest;Trusted_Connection=True;")
                                                                          .Options;

        using var context = new TomeshelfMcmDbContext(options);

        // Act
        var eventEntity = context.Model.FindEntityType(typeof(EventEntity));

        // Assert
        eventEntity.ShouldNotBeNull();
        eventEntity!.GetTableName()
                    .ShouldBe("Events");

        var nameProperty = eventEntity.FindProperty(nameof(EventEntity.Name));
        nameProperty.ShouldNotBeNull();
        nameProperty!.GetMaxLength()
                     .ShouldBe(30);
        nameProperty.IsNullable.ShouldBeFalse();

        var updatedAtProperty = eventEntity.FindProperty(nameof(EventEntity.UpdatedAt));
        updatedAtProperty.ShouldNotBeNull();
        updatedAtProperty!.IsNullable.ShouldBeFalse();

        var guestEntity = context.Model.FindEntityType(typeof(GuestEntity));
        guestEntity.ShouldNotBeNull();
        guestEntity!.GetTableName()
                    .ShouldBe("Guests");

        var addedAtProperty = guestEntity.FindProperty(nameof(GuestEntity.AddedAt));
        addedAtProperty.ShouldNotBeNull();
        addedAtProperty!.IsNullable.ShouldBeFalse();
        addedAtProperty.GetDefaultValueSql()
                       .ShouldBe("SYSDATETIMEOFFSET()");

        guestEntity.FindProperty(nameof(GuestEntity.RemovedAt))
                  ?.IsNullable
                   .ShouldBeTrue();

        var guestInfoEntity = context.Model.FindEntityType(typeof(GuestInfoEntity));
        guestInfoEntity.ShouldNotBeNull();
        guestInfoEntity!.GetTableName()
                        .ShouldBe("GuestInformation");

        var guestSocialEntity = context.Model.FindEntityType(typeof(GuestSocial));
        guestSocialEntity.ShouldNotBeNull();
        guestSocialEntity!.GetTableName()
                          .ShouldBe("GuestSocials");

        guestEntity.GetForeignKeys()
                   .ShouldContain(fk => fk.PrincipalEntityType == eventEntity);
    }
}