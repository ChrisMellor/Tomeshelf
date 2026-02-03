using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.MCM.Domain.Mcm;
using Tomeshelf.MCM.Infrastructure;

namespace Tomeshelf.MCM.Infrastructure.Tests;

public class TomeshelfMcmDbContextTests
{
    [Fact]
    public void Entities_AreConfiguredWithExpectedMetadata()
    {
        var options = new DbContextOptionsBuilder<TomeshelfMcmDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=McmModelTest;Trusted_Connection=True;")
            .Options;

        using var context = new TomeshelfMcmDbContext(options);
        var eventEntity = context.Model.FindEntityType(typeof(EventEntity));
        eventEntity.Should().NotBeNull();
        eventEntity!.GetTableName().Should().Be("Events");

        var nameProperty = eventEntity.FindProperty(nameof(EventEntity.Name));
        nameProperty.Should().NotBeNull();
        nameProperty!.GetMaxLength().Should().Be(30);
        nameProperty.IsNullable.Should().BeFalse();

        var updatedAtProperty = eventEntity.FindProperty(nameof(EventEntity.UpdatedAt));
        updatedAtProperty.Should().NotBeNull();
        updatedAtProperty!.IsNullable.Should().BeFalse();

        var guestEntity = context.Model.FindEntityType(typeof(GuestEntity));
        guestEntity.Should().NotBeNull();
        guestEntity!.GetTableName().Should().Be("Guests");

        var addedAtProperty = guestEntity.FindProperty(nameof(GuestEntity.AddedAt));
        addedAtProperty.Should().NotBeNull();
        addedAtProperty!.IsNullable.Should().BeFalse();
        addedAtProperty.GetDefaultValueSql().Should().Be("SYSDATETIMEOFFSET()");

        guestEntity.FindProperty(nameof(GuestEntity.RemovedAt))
                   ?.IsNullable
                   .Should()
                   .BeTrue();

        var guestInfoEntity = context.Model.FindEntityType(typeof(GuestInfoEntity));
        guestInfoEntity.Should().NotBeNull();
        guestInfoEntity!.GetTableName().Should().Be("GuestInformation");

        var guestSocialEntity = context.Model.FindEntityType(typeof(GuestSocial));
        guestSocialEntity.Should().NotBeNull();
        guestSocialEntity!.GetTableName().Should().Be("GuestSocials");

        guestEntity.GetForeignKeys()
                   .Should()
                   .Contain(fk => fk.PrincipalEntityType == eventEntity);
    }
}
