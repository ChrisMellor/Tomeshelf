using Microsoft.Extensions.Caching.Memory;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.FitbitAuthorizationServiceTests;

public class BuildAuthorizationUri
{
    [Fact]
    public void StoresStateAndDefaultReturnUrl()
    {
        // Arrange
        var options = new FitbitOptions
        {
            ClientId = "client",
            ClientSecret = "secret",
            Scope = "activity",
            CallbackBaseUri = "https://example.test",
            CallbackPath = "/oauth/callback"
        };
        var httpContext = FitbitAuthorizationServiceTestHarness.CreateSessionContext();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = FitbitAuthorizationServiceTestHarness.CreateService(options, httpContext, cache);

        // Act
        var uri = service.BuildAuthorizationUri(null, out var state);

        // Assert
        string.IsNullOrWhiteSpace(state).ShouldBeFalse();
        var uriString = uri.ToString();
        uriString.ShouldContain("client_id=client");
        uriString.ShouldContain("redirect_uri=https%3A%2F%2Fexample.test%2Foauth%2Fcallback");
        uriString.ShouldContain("state=");
        service.TryConsumeState(state, out var codeVerifier, out var returnUrl).ShouldBeTrue();
        returnUrl.ShouldBe("/fitness");
        string.IsNullOrWhiteSpace(codeVerifier).ShouldBeFalse();
        service.TryConsumeState(state, out _, out _).ShouldBeFalse();
    }
}
