using System.Text;
using Bogus;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Tomeshelf.Fitbit.Infrastructure;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.FitbitTokenCacheTests;

public class RefreshToken
{
    [Fact]
    public void WhenInSession_ReturnsValue()
    {
        // Arrange
        var faker = new Faker();
        var token = faker.Random.AlphaNumeric(16);
        var bytes = Encoding.UTF8.GetBytes(token);
        var (cache, session) = CreateCache();

        byte[]? stored = null;
        A.CallTo(() => session.TryGetValue("fitbit_refresh_token", out stored))
            .Returns(true)
            .AssignsOutAndRefParameters(bytes);

        // Act
        var result = cache.RefreshToken;

        // Assert
        result.Should().Be(token);
    }

    [Fact]
    public void WhenNotInSession_ReturnsNull()
    {
        // Arrange
        var (cache, session) = CreateCache();

        byte[]? stored = null;
        A.CallTo(() => session.TryGetValue("fitbit_refresh_token", out stored))
            .Returns(false);

        // Act
        var result = cache.RefreshToken;

        // Assert
        result.Should().BeNull();
    }

    private static (FitbitTokenCache Cache, ISession Session) CreateCache()
    {
        var accessor = A.Fake<IHttpContextAccessor>();
        var context = A.Fake<HttpContext>();
        var session = A.Fake<ISession>();

        A.CallTo(() => accessor.HttpContext).Returns(context);
        A.CallTo(() => context.Session).Returns(session);
        A.CallTo(() => session.IsAvailable).Returns(true);

        return (new FitbitTokenCache(accessor), session);
    }
}
