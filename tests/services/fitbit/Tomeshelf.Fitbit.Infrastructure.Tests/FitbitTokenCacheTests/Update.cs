using System.Text;
using Bogus;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Tomeshelf.Fitbit.Infrastructure;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.FitbitTokenCacheTests;

public class Update
{
    [Fact]
    public void SetsValuesInSession()
    {
        // Arrange
        var faker = new Faker();
        var accessToken = faker.Random.AlphaNumeric(16);
        var refreshToken = faker.Random.AlphaNumeric(16);
        var expiresAt = faker.Date.SoonOffset();
        var (cache, session) = CreateCache();

        // Act
        cache.Update(accessToken, refreshToken, expiresAt);

        // Assert
        A.CallTo(() => session.Set("fitbit_access_token", A<byte[]>.That.Matches(b => Encoding.UTF8.GetString(b) == accessToken)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => session.Set("fitbit_refresh_token", A<byte[]>.That.Matches(b => Encoding.UTF8.GetString(b) == refreshToken)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => session.Set("fitbit_expires_at", A<byte[]>.That.Matches(b => Encoding.UTF8.GetString(b) == expiresAt.ToString("O"))))
            .MustHaveHappenedOnceExactly();
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
