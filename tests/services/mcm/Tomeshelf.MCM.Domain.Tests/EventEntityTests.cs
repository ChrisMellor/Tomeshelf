using System;
using System.Collections.Generic;
using Xunit;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Domain.Tests;

public class EventEntityTests
{
    [Fact]
    public void GetUniqueGuestCount_NoGuests_ReturnsZero()
    {
        // Arrange
        var eventEntity = new EventEntity();

        // Act
        var uniqueGuestCount = eventEntity.GetUniqueGuestCount();

        // Assert
        Assert.Equal(0, uniqueGuestCount);
    }

    [Fact]
    public void GetUniqueGuestCount_WithUniqueGuests_ReturnsCorrectCount()
    {
        // Arrange
        var eventEntity = new EventEntity
        {
            Guests = new List<GuestEntity>
            {
                new GuestEntity { Id = Guid.Parse("00000000-0000-0000-0000-000000000001") },
                new GuestEntity { Id = Guid.Parse("00000000-0000-0000-0000-000000000002") },
                new GuestEntity { Id = Guid.Parse("00000000-0000-0000-0000-000000000003") }
            }
        };

        // Act
        var uniqueGuestCount = eventEntity.GetUniqueGuestCount();

        // Assert
        Assert.Equal(3, uniqueGuestCount);
    }

    [Fact]
    public void GetUniqueGuestCount_WithDuplicateGuests_ReturnsCorrectCount()
    {
        // Arrange
        var eventEntity = new EventEntity
        {
            Guests = new List<GuestEntity>
            {
                new GuestEntity { Id = Guid.Parse("00000000-0000-0000-0000-000000000001") },
                new GuestEntity { Id = Guid.Parse("00000000-0000-0000-0000-000000000002") },
                new GuestEntity { Id = Guid.Parse("00000000-0000-0000-0000-000000000001") }
            }
        };

        // Act
        var uniqueGuestCount = eventEntity.GetUniqueGuestCount();

        // Assert
        Assert.Equal(2, uniqueGuestCount);
    }

    [Fact]
    public void GetUniqueGuestCount_EmptyGuestId_CountsAsUnique()
    {
        // Arrange
        var eventEntity = new EventEntity
        {
            Guests = new List<GuestEntity>
            {
                new GuestEntity { Id = Guid.Parse("00000000-0000-0000-0000-000000000001") },
                new GuestEntity { Id = Guid.Parse("00000000-0000-0000-0000-000000000000") }, // Empty Guid for empty string
                new GuestEntity { Id = Guid.Parse("00000000-0000-0000-0000-000000000003") },
                new GuestEntity { Id = Guid.Parse("00000000-0000-0000-0000-000000000000") }  // Empty Guid for empty string
            }
        };

        // Act
        var uniqueGuestCount = eventEntity.GetUniqueGuestCount();

        // Assert
        Assert.Equal(3, uniqueGuestCount);
    }
}
