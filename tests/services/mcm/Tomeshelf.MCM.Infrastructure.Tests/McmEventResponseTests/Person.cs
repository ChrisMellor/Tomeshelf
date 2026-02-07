using Tomeshelf.MCM.Infrastructure.Responses;

namespace Tomeshelf.MCM.Infrastructure.Tests.McmEventResponseTests;

public class Person
{
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
        var imdb = "tt1234567";
        var youtube = "john_doe_yt";
        var twitch = "john_doe_twitch";
        var snapchat = "john_doe_snap";
        var deviantArt = "john_doe_da";
        var tumblr = "john_doe_tumblr";
        var fandom = "fandom-name";
        var tikTok = "john_doe_tiktok";
        var category = "Actor";
        var daysAtShow = "Fri, Sat";
        var boothNumber = "123";
        var autographAmount = "10.50";
        var photoOpAmount = "20.00";
        var photoOpTableAmount = "15.00";
        var peopleCategories = new object[] { "Category1", "Category2" };
        var globalCategories = new McmEventResponse.GlobalCategory[] { new McmEventResponse.GlobalCategory { Id = "cat-id-1", Name = "Category A" } };
        var images = new McmEventResponse.Image[] { new McmEventResponse.Image { Big = "big.jpg" } };
        var discoverable = true;
        var epicPhotoUrl = "http://epicphoto.com";
        var sort = new { Field = "Name", Order = "asc" };

        // Act
        var person = new McmEventResponse.Person
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
            Imdb = imdb,
            YouTube = youtube,
            Twitch = twitch,
            Snapchat = snapchat,
            DeviantArt = deviantArt,
            Tumblr = tumblr,
            Fandom = fandom,
            TikTok = tikTok,
            Category = category,
            DaysAtShow = daysAtShow,
            BoothNumber = boothNumber,
            AutographAmount = autographAmount,
            PhotoOpAmount = photoOpAmount,
            PhotoOpTableAmount = photoOpTableAmount,
            PeopleCategories = peopleCategories,
            GlobalCategories = globalCategories,
            Images = images,
            Discoverable = discoverable,
            EpicPhotoUrl = epicPhotoUrl,
            Sort = sort
        };

        // Assert
        person.Id.ShouldBe(id);
        person.Uid.ShouldBe(uid);
        person.PubliclyVisible.ShouldBe(publiclyVisible);
        person.FirstName.ShouldBe(firstName);
        person.LastName.ShouldBe(lastName);
        person.AltName.ShouldBe(altName);
        person.Bio.ShouldBe(bio);
        person.KnownFor.ShouldBe(knownFor);
        person.ProfileUrl.ShouldBe(profileUrl);
        person.ProfileUrlLabel.ShouldBe(profileUrlLabel);
        person.VideoLink.ShouldBe(videoLink);
        person.Twitter.ShouldBe(twitter);
        person.Facebook.ShouldBe(facebook);
        person.Instagram.ShouldBe(instagram);
        person.Imdb.ShouldBe(imdb);
        person.YouTube.ShouldBe(youtube);
        person.Twitch.ShouldBe(twitch);
        person.Snapchat.ShouldBe(snapchat);
        person.DeviantArt.ShouldBe(deviantArt);
        person.Tumblr.ShouldBe(tumblr);
        person.Fandom.ShouldBe(fandom);
        person.TikTok.ShouldBe(tikTok);
        person.Category.ShouldBe(category);
        person.DaysAtShow.ShouldBe(daysAtShow);
        person.BoothNumber.ShouldBe(boothNumber);
        person.AutographAmount.ShouldBe(autographAmount);
        person.PhotoOpAmount.ShouldBe(photoOpAmount);
        person.PhotoOpTableAmount.ShouldBe(photoOpTableAmount);
        person.PeopleCategories.ShouldBeSameAs(peopleCategories);
        person.GlobalCategories.ShouldBeSameAs(globalCategories);
        person.Images.ShouldBeSameAs(images);
        person.Discoverable.ShouldBe(discoverable);
        person.EpicPhotoUrl.ShouldBe(epicPhotoUrl);
        person.Sort.ShouldBe(sort);
    }
}
