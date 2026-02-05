using Bogus;
using FluentAssertions;
using Tomeshelf.MCM.Application.Mappers;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Application.Tests.Mappers.GuestMapperTests;

public class CloneForEvent
{
    [Fact]
    public void CopiesIdentifiersAndSocials()
    {
        // Arrange
        var faker = new Faker();
        var mapper = new GuestMapper();
        var guestId = Guid.NewGuid();
        var infoId = Guid.NewGuid();
        var socialId = Guid.NewGuid();
        var eventId = faker.Random.AlphaNumeric(8);
        var firstName = faker.Name.FirstName();
        var lastName = faker.Name.LastName();
        var source = new GuestEntity
        {
            Id = guestId,
            Information = new GuestInfoEntity
            {
                Id = infoId,
                FirstName = firstName,
                LastName = lastName,
                Socials = new GuestSocial
                {
                    Id = socialId,
                    Twitter = faker.Internet.UserName()
                }
            }
        };

        // Act
        var cloned = mapper.CloneForEvent(eventId, source);

        // Assert
        cloned.EventId
              .Should()
              .Be(eventId);
        cloned.Id
              .Should()
              .Be(guestId);
        cloned.GuestInfoId
              .Should()
              .Be(infoId);
        cloned.Information
              .Should()
              .NotBeNull();
        cloned.Information!.FirstName
              .Should()
              .Be(firstName);
        cloned.Information
              .LastName
              .Should()
              .Be(lastName);
        cloned.Information
              .Socials
              .Should()
              .NotBeNull();
        cloned.Information.Socials!.Id
              .Should()
              .Be(socialId);
        cloned.Information
              .Socials
              .Twitter
              .Should()
              .Be(source.Information.Socials!.Twitter);
        cloned.IsDeleted
              .Should()
              .BeFalse();
    }
}