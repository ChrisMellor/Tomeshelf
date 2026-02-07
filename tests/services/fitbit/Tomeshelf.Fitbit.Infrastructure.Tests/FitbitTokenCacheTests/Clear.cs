using FakeItEasy;
using Microsoft.AspNetCore.Http;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.FitbitTokenCacheTests;

public class Clear
{
    [Fact]
    public void RemovesValuesFromSession()
    {
        var (cache, session) = CreateCache();

        cache.Clear();

        A.CallTo(() => session.Remove("fitbit_access_token"))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => session.Remove("fitbit_refresh_token"))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => session.Remove("fitbit_expires_at"))
         .MustHaveHappenedOnceExactly();
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