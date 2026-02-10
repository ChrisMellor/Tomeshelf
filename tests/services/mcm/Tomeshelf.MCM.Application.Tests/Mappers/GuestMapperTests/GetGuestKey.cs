using Bogus;
using Shouldly;
using Tomeshelf.MCM.Application.Mappers;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Application.Tests.Mappers.GuestMapperTests;

public class GetGuestKey
{
    /// <summary>
    ///     Trims the names and combines.
    /// </summary>
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
        key.ShouldBe($"{firstName} {lastName}");
    }

    /// <summary>
    ///     Returns empty when the names are missing.
    /// </summary>
    [Fact]
    public void WhenNamesMissing_ReturnsEmpty()
    {
        // Arrange
        var mapper = new GuestMapper();
        var guest = new GuestEntity { Information = new GuestInfoEntity() };

        // Act
        var key = mapper.GetGuestKey(guest);

        // Assert
        key.ShouldBe(string.Empty);
    }
}