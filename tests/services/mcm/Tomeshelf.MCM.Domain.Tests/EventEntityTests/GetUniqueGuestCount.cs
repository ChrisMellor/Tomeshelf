using Shouldly;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Domain.Tests.EventEntityTests;

public class GetUniqueGuestCount
{
    /// <summary>
    ///     Excludes the deleted guests.
    /// </summary>
    [Fact]
    public void ExcludesDeletedGuests()
    {
        // Arrange
        var eventEntity = new EventEntity
        {
            Guests = new List<GuestEntity>
            {
                new GuestEntity
                {
                    Id = Guid.NewGuid(),
                    Information = new GuestInfoEntity
                    {
                        FirstName = "John",
                        LastName = "Doe"
                    }
                },
                new GuestEntity
                {
                    Id = Guid.NewGuid(),
                    Information = new GuestInfoEntity
                    {
                        FirstName = "Jane",
                        LastName = "Doe"
                    },
                    IsDeleted = true
                },
                new GuestEntity
                {
                    Id = Guid.NewGuid(),
                    Information = new GuestInfoEntity
                    {
                        FirstName = "Jim",
                        LastName = "Smith"
                    }
                }
            }
        };

        // Act
        var result = eventEntity.GetUniqueGuestCount();

        // Assert
        result.ShouldBe(2);
    }

    /// <summary>
    ///     Returns the correct count.
    /// </summary>
    [Fact]
    public void ReturnsCorrectCount()
    {
        // Arrange
        var eventEntity = new EventEntity
        {
            Guests = new List<GuestEntity>
            {
                new GuestEntity
                {
                    Id = Guid.NewGuid(),
                    Information = new GuestInfoEntity
                    {
                        FirstName = "John",
                        LastName = "Doe"
                    }
                },
                new GuestEntity
                {
                    Id = Guid.NewGuid(),
                    Information = new GuestInfoEntity
                    {
                        FirstName = "Jane",
                        LastName = "Doe"
                    }
                },
                new GuestEntity
                {
                    Id = Guid.NewGuid(),
                    Information = new GuestInfoEntity
                    {
                        FirstName = "John",
                        LastName = "Doe"
                    }
                }
            }
        };

        // Act
        var result = eventEntity.GetUniqueGuestCount();

        // Assert
        result.ShouldBe(2);
    }

    /// <summary>
    ///     Returns zero when there are no guests.
    /// </summary>
    [Fact]
    public void ReturnsZero_WhenNoGuests()
    {
        // Arrange
        var eventEntity = new EventEntity { Guests = new List<GuestEntity>() };

        // Act
        var result = eventEntity.GetUniqueGuestCount();

        // Assert
        result.ShouldBe(0);
    }
}