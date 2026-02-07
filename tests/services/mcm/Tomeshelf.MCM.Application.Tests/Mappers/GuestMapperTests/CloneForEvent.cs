using Bogus;
using Shouldly;
using Tomeshelf.MCM.Application.Mappers;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Application.Tests.Mappers.GuestMapperTests;

public class CloneForEvent
{
    [Fact]
    public void CopiesIdentifiersAndSocials()
    {
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

        var cloned = mapper.CloneForEvent(eventId, source);

        cloned.EventId.ShouldBe(eventId);
        cloned.Id.ShouldBe(guestId);
        cloned.GuestInfoId.ShouldBe(infoId);
        cloned.Information.ShouldNotBeNull();
        cloned.Information!.FirstName.ShouldBe(firstName);
        cloned.Information.LastName.ShouldBe(lastName);
        cloned.Information.Socials.ShouldNotBeNull();
        cloned.Information.Socials!.Id.ShouldBe(socialId);
        cloned.Information.Socials.Twitter.ShouldBe(source.Information.Socials!.Twitter);
        cloned.IsDeleted.ShouldBeFalse();
    }
}