using Bogus;
using FluentAssertions;
using Tomeshelf.MCM.Application.Mappers;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Application.Tests.Mappers.GuestMapperTests;

public class UpdateGuest
{
    [Fact]
    public void WhenSourceInfoMissing_ReturnsFalse()
    {
        // Arrange
        var faker = new Faker();
        var mapper = new GuestMapper();
        var target = new GuestEntity
        {
            Id = Guid.NewGuid(),
            Information = new GuestInfoEntity { FirstName = faker.Name.FirstName() }
        };
        var originalFirstName = target.Information.FirstName;
        var source = new GuestEntity { Information = null };

        // Act
        var changed = mapper.UpdateGuest(target, source);

        // Assert
        changed.Should().BeFalse();
        target.Information!.FirstName.Should().Be(originalFirstName);
    }

    [Fact]
    public void WhenDeleted_ReactivatesAndUpdatesInformation()
    {
        // Arrange
        var faker = new Faker();
        var mapper = new GuestMapper();
        var target = new GuestEntity
        {
            Id = Guid.NewGuid(),
            GuestInfoId = Guid.Empty,
            IsDeleted = true,
            RemovedAt = DateTimeOffset.UtcNow.AddDays(-1),
            AddedAt = default,
            Information = null
        };
        var source = new GuestEntity
        {
            Information = new GuestInfoEntity
            {
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Bio = faker.Lorem.Sentence(),
                Socials = new GuestSocial { Imdb = faker.Internet.Url() }
            }
        };

        // Act
        var changed = mapper.UpdateGuest(target, source);

        // Assert
        changed.Should().BeTrue();
        target.IsDeleted.Should().BeFalse();
        target.RemovedAt.Should().BeNull();
        target.AddedAt.Should().NotBe(default);
        target.Information.Should().NotBeNull();
        target.Information!.FirstName.Should().Be(source.Information!.FirstName);
        target.Information.LastName.Should().Be(source.Information.LastName);
        target.Information.Bio.Should().Be(source.Information.Bio);
        target.Information.Socials.Should().NotBeNull();
        target.Information.Socials!.Imdb.Should().Be(source.Information.Socials!.Imdb);
    }
}
