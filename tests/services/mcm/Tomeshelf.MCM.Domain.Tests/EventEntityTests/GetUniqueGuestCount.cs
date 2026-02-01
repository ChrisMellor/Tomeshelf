using FluentAssertions;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Domain.Tests.EventEntityTests;

public class GetUniqueGuestCount
{
    [Fact]
    public void NoGuests_ReturnsZero()
    {
        // Arrange
        var eventEntity = new EventEntity();

        // Act
        var uniqueGuestCount = eventEntity.GetUniqueGuestCount();

        // Assert
        uniqueGuestCount.Should().Be(0);
    }

    [Fact]
    public void WithUniqueGuests_ReturnsCorrectCount()
    {
        // Arrange
        var guestA = Guid.NewGuid();
        var guestB = Guid.NewGuid();
        var guestC = Guid.NewGuid();
        var eventEntity = new EventEntity
        {
            Guests = new List<GuestEntity>
            {
                new() { Id = guestA },
                new() { Id = guestB },
                new() { Id = guestC }
            }
        };

        // Act
        var uniqueGuestCount = eventEntity.GetUniqueGuestCount();

        // Assert
        uniqueGuestCount.Should().Be(3);
    }

    [Fact]
    public void WithDuplicateGuests_ReturnsCorrectCount()
    {
        // Arrange
        var guestA = Guid.NewGuid();
        var guestB = Guid.NewGuid();
        var eventEntity = new EventEntity
        {
            Guests = new List<GuestEntity>
            {
                new() { Id = guestA },
                new() { Id = guestB },
                new() { Id = guestA }
            }
        };

        // Act
        var uniqueGuestCount = eventEntity.GetUniqueGuestCount();

        // Assert
        uniqueGuestCount.Should().Be(2);
    }

    [Fact]
    public void EmptyGuestId_CountsAsUnique()
    {
        // Arrange
        var guestA = Guid.NewGuid();
        var guestB = Guid.NewGuid();
        var eventEntity = new EventEntity
        {
            Guests = new List<GuestEntity>
            {
                new() { Id = guestA },
                new() { Id = Guid.Empty },
                new() { Id = guestB },
                new() { Id = Guid.Empty }
            }
        };

        // Act
        var uniqueGuestCount = eventEntity.GetUniqueGuestCount();

        // Assert
        uniqueGuestCount.Should().Be(3);
    }
}
