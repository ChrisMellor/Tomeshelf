using System;
using Tomeshelf.MCM.Application.Mappers;
using Tomeshelf.MCM.Domain.Mcm;
using Xunit;

namespace Tomeshelf.MCM.Application.Tests.Mappers;

public class GuestMapperTests
{
    [Fact]
    public void GetGuestKey_TrimsNamesAndCombines()
    {
        // Arrange
        var mapper = new GuestMapper();
        var guest = new GuestEntity
        {
            Information = new GuestInfoEntity
            {
                FirstName = "  Jane ",
                LastName = " Doe  "
            }
        };

        // Act
        var key = mapper.GetGuestKey(guest);

        // Assert
        Assert.Equal("Jane Doe", key);
    }

    [Fact]
    public void GetGuestKey_WhenNamesMissing_ReturnsEmpty()
    {
        // Arrange
        var mapper = new GuestMapper();
        var guest = new GuestEntity { Information = new GuestInfoEntity() };

        // Act
        var key = mapper.GetGuestKey(guest);

        // Assert
        Assert.Equal(string.Empty, key);
    }

    [Fact]
    public void CloneForEvent_CopiesIdentifiersAndSocials()
    {
        // Arrange
        var mapper = new GuestMapper();
        var guestId = Guid.NewGuid();
        var infoId = Guid.NewGuid();
        var socialId = Guid.NewGuid();
        var source = new GuestEntity
        {
            Id = guestId,
            Information = new GuestInfoEntity
            {
                Id = infoId,
                FirstName = "Jane",
                LastName = "Doe",
                Socials = new GuestSocial
                {
                    Id = socialId,
                    Twitter = "@jane"
                }
            }
        };

        // Act
        var cloned = mapper.CloneForEvent("event-1", source);

        // Assert
        Assert.Equal("event-1", cloned.EventId);
        Assert.Equal(guestId, cloned.Id);
        Assert.Equal(infoId, cloned.GuestInfoId);
        Assert.NotNull(cloned.Information);
        Assert.Equal("Jane", cloned.Information.FirstName);
        Assert.Equal("Doe", cloned.Information.LastName);
        Assert.NotNull(cloned.Information.Socials);
        Assert.Equal(socialId, cloned.Information.Socials.Id);
        Assert.Equal("@jane", cloned.Information.Socials.Twitter);
        Assert.False(cloned.IsDeleted);
    }

    [Fact]
    public void UpdateGuest_WhenSourceInfoMissing_ReturnsFalse()
    {
        // Arrange
        var mapper = new GuestMapper();
        var target = new GuestEntity
        {
            Id = Guid.NewGuid(),
            Information = new GuestInfoEntity { FirstName = "Old" }
        };
        var source = new GuestEntity { Information = null };

        // Act
        var changed = mapper.UpdateGuest(target, source);

        // Assert
        Assert.False(changed);
        Assert.Equal("Old", target.Information.FirstName);
    }

    [Fact]
    public void UpdateGuest_WhenDeleted_ReactivatesAndUpdatesInformation()
    {
        // Arrange
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
                FirstName = "New",
                LastName = "Name",
                Bio = "Bio",
                Socials = new GuestSocial { Imdb = "https://imdb.example" }
            }
        };

        // Act
        var changed = mapper.UpdateGuest(target, source);

        // Assert
        Assert.True(changed);
        Assert.False(target.IsDeleted);
        Assert.Null(target.RemovedAt);
        Assert.NotEqual(default, target.AddedAt);
        Assert.NotNull(target.Information);
        Assert.Equal("New", target.Information.FirstName);
        Assert.Equal("Name", target.Information.LastName);
        Assert.Equal("Bio", target.Information.Bio);
        Assert.NotNull(target.Information.Socials);
        Assert.Equal("https://imdb.example", target.Information.Socials.Imdb);
    }
}
