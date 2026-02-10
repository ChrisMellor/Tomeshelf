using FakeItEasy;
using Microsoft.AspNetCore.Http;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.FitbitTokenCacheTests;

public class Clear
{
    /// <summary>
    ///     Removes the values from session.
    /// </summary>
    [Fact]
    public void RemovesValuesFromSession()
    {
        // Arrange
        var (cache, session) = CreateCache();

        // Act
        cache.Clear();

        // Assert
        A.CallTo(() => session.Remove("fitbit_access_token"))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => session.Remove("fitbit_refresh_token"))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => session.Remove("fitbit_expires_at"))
         .MustHaveHappenedOnceExactly();
    }

    /// <summary>
    ///     Creates the cache.
    /// </summary>
    /// <returns>The result of the operation.</returns>
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