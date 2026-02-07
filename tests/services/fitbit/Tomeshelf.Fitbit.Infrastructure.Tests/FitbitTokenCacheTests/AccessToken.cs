using System.Text;
using Bogus;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.FitbitTokenCacheTests;

public class AccessToken
{
    [Fact]
    public void WhenInSession_ReturnsValue()
    {
        var faker = new Faker();
        var token = faker.Random.AlphaNumeric(16);
        var bytes = Encoding.UTF8.GetBytes(token);
        var (cache, session) = CreateCache();

        byte[]? stored = null;
        A.CallTo(() => session.TryGetValue("fitbit_access_token", out stored))
         .Returns(true)
         .AssignsOutAndRefParameters(bytes);

        var result = cache.AccessToken;

        result.ShouldBe(token);
    }

    [Fact]
    public void WhenNotInSession_ReturnsNull()
    {
        var (cache, session) = CreateCache();

        byte[]? stored = null;
        A.CallTo(() => session.TryGetValue("fitbit_access_token", out stored))
         .Returns(false);

        var result = cache.AccessToken;

        result.ShouldBeNull();
    }

    private static (FitbitTokenCache Cache, ISession Session) CreateCache()
    {
        var accessor = A.Fake<IHttpContextAccessor>();
        var context = A.Fake<HttpContext>();
        var session = A.Fake<ISession>();

        A.CallTo(() => accessor.HttpContext)
         .Returns(context);
        A.CallTo(() => context.Session)
         .Returns(session);
        A.CallTo(() => session.IsAvailable)
         .Returns(true);

        return (new FitbitTokenCache(accessor), session);
    }
}