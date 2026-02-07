using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Shouldly;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.FitbitAuthorizationServiceTests;

public class ExchangeAuthorizationCodeAsync
{
    [Fact]
    public async Task Throws_WhenClientCredentialsMissing()
    {
        var options = new FitbitOptions
        {
            ClientId = string.Empty,
            ClientSecret = string.Empty,
            CallbackBaseUri = "https://example.test"
        };
        var httpContext = FitbitAuthorizationServiceTestHarness.CreateSessionContext();
        var service = FitbitAuthorizationServiceTestHarness.CreateService(options, httpContext, new MemoryCache(new MemoryCacheOptions()));

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => service.ExchangeAuthorizationCodeAsync("code", "verifier", CancellationToken.None));

        exception.Message.ShouldBe("Fitbit client credentials are not configured.");
    }

    [Fact]
    public async Task Throws_WhenCodeVerifierMissing()
    {
        var options = new FitbitOptions
        {
            ClientId = "client",
            ClientSecret = "secret",
            CallbackBaseUri = "https://example.test"
        };
        var httpContext = FitbitAuthorizationServiceTestHarness.CreateSessionContext();
        var service = FitbitAuthorizationServiceTestHarness.CreateService(options, httpContext, new MemoryCache(new MemoryCacheOptions()));

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => service.ExchangeAuthorizationCodeAsync("code", "", CancellationToken.None));

        exception.Message.ShouldBe("Missing PKCE code verifier for Fitbit authorization exchange.");
    }

    [Fact]
    public async Task UpdatesTokenCache_OnSuccess()
    {
        var options = new FitbitOptions
        {
            ClientId = "client",
            ClientSecret = "secret",
            CallbackBaseUri = "https://example.test",
            CallbackPath = "/api/fitbit/auth/callback"
        };
        var httpContext = FitbitAuthorizationServiceTestHarness.CreateSessionContext();
        var cache = new MemoryCache(new MemoryCacheOptions());

        var payload = "{\"access_token\":\"access\",\"refresh_token\":\"refresh\",\"expires_in\":3600}";
        var client = FitbitAuthorizationServiceTestHarness.CreateSuccessClient(payload);
        var service = FitbitAuthorizationServiceTestHarness.CreateService(options, httpContext, cache, client);

        await service.ExchangeAuthorizationCodeAsync("code", "verifier", CancellationToken.None);

        var tokenCache = new FitbitTokenCache(new HttpContextAccessor { HttpContext = httpContext });
        tokenCache.AccessToken.ShouldBe("access");
        tokenCache.RefreshToken.ShouldBe("refresh");
        tokenCache.ExpiresAtUtc.ShouldNotBeNull();
    }
}