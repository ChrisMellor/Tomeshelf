using Bogus;
using FluentAssertions;
using Tomeshelf.MCM.Application.Mappers;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Application.Tests.Mappers.GuestMapperTests;

public class GetGuestKey
{
    [Fact]
    public void TrimsNamesAndCombines()
    {
        // Arrange
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

        // Act
        var key = mapper.GetGuestKey(guest);

        // Assert
        key.Should()
           .Be($"{firstName} {lastName}");
    }

    [Fact]
    public void WhenNamesMissing_ReturnsEmpty()
    {
        // Arrange
        var mapper = new GuestMapper();
        var guest = new GuestEntity { Information = new GuestInfoEntity() };

        // Act
        var key = mapper.GetGuestKey(guest);

        // Assert
        key.Should()
           .BeEmpty();
    }
}