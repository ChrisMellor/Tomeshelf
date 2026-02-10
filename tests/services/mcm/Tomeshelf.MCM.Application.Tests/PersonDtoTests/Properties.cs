using Shouldly;
using Tomeshelf.MCM.Application.Mcm;

namespace Tomeshelf.MCM.Application.Tests.PersonDtoTests;

public class Properties
{
    /// <summary>
    ///     Determines whether the current instance can set and get values.
    /// </summary>
    [Fact]
    public void CanSetAndGetValues()
    {
        // Arrange
        var id = "person-id-1";
        var uid = "person-uid-1";
        var publiclyVisible = true;
        var firstName = "John";
        var lastName = "Doe";
        var altName = "Johnny";
        var bio = "A person's biography.";
        var knownFor = "Known for many things.";
        var profileUrl = "http://profile.com";
        var profileUrlLabel = "Profile";
        var videoLink = "http://video.com";
        var twitter = "john_doe";
        var facebook = "john.doe";
        var instagram = "john.doe.ig";
        var youtube = "john_doe_yt";
        var twitch = "john_doe_twitch";
        var snapchat = "john_doe_snap";
        var deviantArt = "john_doe_da";
        var tumblr = "john_doe_tumblr";
        var category = "Actor";
        var daysAtShow = "Fri, Sat";
        var boothNumber = "123";
        var autographAmount = 10.50m;
        var photoOpAmount = 20.00m;
        var photoOpTableAmount = 15.00m;
        var peopleCategories = new List<object>
        {
            "Category1",
            "Category2"
        };
        var globalCategories = new List<CategoryDto>
        {
            new()
            {
                Id = "cat-id-1",
                Name = "Category A"
            }
        };
        var images = new List<ImageSetDto> { new() { Big = "big.jpg" } };
        var schedules = new List<ScheduleDto>
        {
            new()
            {
                Id = "sched-id-1",
                Title = "Panel"
            }
        };
        var removedAt = "2025-01-01";

        // Act
        var dto = new PersonDto
        {
            Id = id,
            Uid = uid,
            PubliclyVisible = publiclyVisible,
            FirstName = firstName,
            LastName = lastName,
            AltName = altName,
            Bio = bio,
            KnownFor = knownFor,
            ProfileUrl = profileUrl,
            ProfileUrlLabel = profileUrlLabel,
            VideoLink = videoLink,
            Twitter = twitter,
            Facebook = facebook,
            Instagram = instagram,
            YouTube = youtube,
            Twitch = twitch,
            Snapchat = snapchat,
            DeviantArt = deviantArt,
            Tumblr = tumblr,
            Category = category,
            DaysAtShow = daysAtShow,
            BoothNumber = boothNumber,
            AutographAmount = autographAmount,
            PhotoOpAmount = photoOpAmount,
            PhotoOpTableAmount = photoOpTableAmount,
            PeopleCategories = peopleCategories,
            GlobalCategories = globalCategories,
            Images = images,
            Schedules = schedules,
            RemovedAt = removedAt
        };

        // Assert
        dto.Id.ShouldBe(id);
        dto.Uid.ShouldBe(uid);
        dto.PubliclyVisible.ShouldBe(publiclyVisible);
        dto.FirstName.ShouldBe(firstName);
        dto.LastName.ShouldBe(lastName);
        dto.AltName.ShouldBe(altName);
        dto.Bio.ShouldBe(bio);
        dto.KnownFor.ShouldBe(knownFor);
        dto.ProfileUrl.ShouldBe(profileUrl);
        dto.ProfileUrlLabel.ShouldBe(profileUrlLabel);
        dto.VideoLink.ShouldBe(videoLink);
        dto.Twitter.ShouldBe(twitter);
        dto.Facebook.ShouldBe(facebook);
        dto.Instagram.ShouldBe(instagram);
        dto.YouTube.ShouldBe(youtube);
        dto.Twitch.ShouldBe(twitch);
        dto.Snapchat.ShouldBe(snapchat);
        dto.DeviantArt.ShouldBe(deviantArt);
        dto.Tumblr.ShouldBe(tumblr);
        dto.Category.ShouldBe(category);
        dto.DaysAtShow.ShouldBe(daysAtShow);
        dto.BoothNumber.ShouldBe(boothNumber);
        dto.AutographAmount.ShouldBe(autographAmount);
        dto.PhotoOpAmount.ShouldBe(photoOpAmount);
        dto.PhotoOpTableAmount.ShouldBe(photoOpTableAmount);
        dto.PeopleCategories.ShouldBeSameAs(peopleCategories);
        dto.GlobalCategories.ShouldBeSameAs(globalCategories);
        dto.Images.ShouldBeSameAs(images);
        dto.Schedules.ShouldBeSameAs(schedules);
        dto.RemovedAt.ShouldBe(removedAt);
    }

    /// <summary>
    ///     Defaults the are expected.
    /// </summary>
    [Fact]
    public void DefaultsAreExpected()
    {
        // Arrange
        var dto = new PersonDto();

        // Act
        var peopleCategories = dto.PeopleCategories;
        var globalCategories = dto.GlobalCategories;
        var images = dto.Images;
        var schedules = dto.Schedules;

        // Assert
        dto.Id.ShouldBeNull();
        dto.Uid.ShouldBeNull();
        dto.PubliclyVisible.ShouldBeFalse();
        dto.FirstName.ShouldBe(string.Empty);
        dto.LastName.ShouldBe(string.Empty);
        dto.AltName.ShouldBeNull();
        dto.Bio.ShouldBeNull();
        dto.KnownFor.ShouldBeNull();
        dto.ProfileUrl.ShouldBeNull();
        dto.ProfileUrlLabel.ShouldBeNull();
        dto.VideoLink.ShouldBeNull();
        dto.Twitter.ShouldBeNull();
        dto.Facebook.ShouldBeNull();
        dto.Instagram.ShouldBeNull();
        dto.YouTube.ShouldBeNull();
        dto.Twitch.ShouldBeNull();
        dto.Snapchat.ShouldBeNull();
        dto.DeviantArt.ShouldBeNull();
        dto.Tumblr.ShouldBeNull();
        dto.Category.ShouldBeNull();
        dto.DaysAtShow.ShouldBeNull();
        dto.BoothNumber.ShouldBeNull();
        dto.AutographAmount.ShouldBeNull();
        dto.PhotoOpAmount.ShouldBeNull();
        dto.PhotoOpTableAmount.ShouldBeNull();
        peopleCategories.ShouldNotBeNull();
        peopleCategories.ShouldBeEmpty();
        globalCategories.ShouldNotBeNull();
        globalCategories.ShouldBeEmpty();
        images.ShouldNotBeNull();
        images.ShouldBeEmpty();
        schedules.ShouldNotBeNull();
        schedules.ShouldBeEmpty();
        dto.RemovedAt.ShouldBeNull();
    }
}