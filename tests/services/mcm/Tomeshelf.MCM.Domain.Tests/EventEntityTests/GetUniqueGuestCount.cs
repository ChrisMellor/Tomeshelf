using Shouldly;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Domain.Tests.EventEntityTests;

public class GetUniqueGuestCount
{
    [Fact]
    public void ExcludesDeletedGuests()
    {
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

        var result = eventEntity.GetUniqueGuestCount();

        result.ShouldBe(2);
    }

    [Fact]
    public void ReturnsCorrectCount()
    {
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

        var result = eventEntity.GetUniqueGuestCount();

        result.ShouldBe(2);
    }

    [Fact]
    public void ReturnsZero_WhenNoGuests()
    {
        var eventEntity = new EventEntity { Guests = new List<GuestEntity>() };

        var result = eventEntity.GetUniqueGuestCount();

        result.ShouldBe(0);
    }
}