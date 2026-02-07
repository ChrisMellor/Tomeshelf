using Bogus;
using Shouldly;
using Tomeshelf.MCM.Application.Mappers;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Application.Tests.Mappers.GuestMapperTests;

public class GetGuestKey
{
    [Fact]
    public void TrimsNamesAndCombines()
    {
        var faker = new Faker();
        var mapper = new GuestMapper();
        var firstName = faker.Name.FirstName();
        var lastName = faker.Name.LastName();
        var guest = new GuestEntity
        {
            Information = new GuestInfoEntity
            {
                FirstName = $"  {firstName} ",
                LastName = $" {lastName}  "
            }
        };

        var key = mapper.GetGuestKey(guest);

        key.ShouldBe($"{firstName} {lastName}");
    }

    [Fact]
    public void WhenNamesMissing_ReturnsEmpty()
    {
        var mapper = new GuestMapper();
        var guest = new GuestEntity { Information = new GuestInfoEntity() };

        var key = mapper.GetGuestKey(guest);

        key.ShouldBe(string.Empty);
    }
}