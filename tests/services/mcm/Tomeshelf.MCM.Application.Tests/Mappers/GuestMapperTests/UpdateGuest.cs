using Bogus;
using Shouldly;
using Tomeshelf.MCM.Application.Mappers;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Application.Tests.Mappers.GuestMapperTests;

public class UpdateGuest
{
    [Fact]
    public void WhenDeleted_ReactivatesAndUpdatesInformation()
    {
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

        var changed = mapper.UpdateGuest(target, source);

        changed.ShouldBeTrue();
        target.IsDeleted.ShouldBeFalse();
        target.RemovedAt.ShouldBeNull();
        target.AddedAt.ShouldNotBe(default);
        target.Information.ShouldNotBeNull();
        target.Information!.FirstName.ShouldBe(source.Information!.FirstName);
        target.Information.LastName.ShouldBe(source.Information.LastName);
        target.Information.Bio.ShouldBe(source.Information.Bio);
        target.Information.Socials.ShouldNotBeNull();
        target.Information.Socials!.Imdb.ShouldBe(source.Information.Socials!.Imdb);
    }

    [Fact]
    public void WhenSourceInfoMissing_ReturnsFalse()
    {
        var faker = new Faker();
        var mapper = new GuestMapper();
        var target = new GuestEntity
        {
            Id = Guid.NewGuid(),
            Information = new GuestInfoEntity { FirstName = faker.Name.FirstName() }
        };
        var originalFirstName = target.Information.FirstName;
        var source = new GuestEntity { Information = null };

        var changed = mapper.UpdateGuest(target, source);

        changed.ShouldBeFalse();
        target.Information!.FirstName.ShouldBe(originalFirstName);
    }
}