using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.FitbitAuthorizationServiceTests;

public class ExchangeAuthorizationCodeAsync
{
    [Fact]
    public async Task Throws_WhenClientCredentialsMissing()
    {
        // Arrange
        var options = new FitbitOptions
        {
            ClientId = string.Empty,
            ClientSecret = string.Empty,
            CallbackBaseUri = "https://example.test"
        };
        var httpContext = FitbitAuthorizationServiceTestHarness.CreateSessionContext();
        var service = FitbitAuthorizationServiceTestHarness.CreateService(options, httpContext, new MemoryCache(new MemoryCacheOptions()));

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => service.ExchangeAuthorizationCodeAsync("code", "verifier", CancellationToken.None));

        // Assert
        exception.Message.ShouldBe("Fitbit client credentials are not configured.");
    }

    [Fact]
    public async Task Throws_WhenCodeVerifierMissing()
    {
        // Arrange
        var options = new FitbitOptions
        {
            ClientId = "client",
            ClientSecret = "secret",
            CallbackBaseUri = "https://example.test"
        };
        var httpContext = FitbitAuthorizationServiceTestHarness.CreateSessionContext();
        var service = FitbitAuthorizationServiceTestHarness.CreateService(options, httpContext, new MemoryCache(new MemoryCacheOptions()));

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => service.ExchangeAuthorizationCodeAsync("code", "", CancellationToken.None));

        // Assert
        exception.Message.ShouldBe("Missing PKCE code verifier for Fitbit authorization exchange.");
    }

    [Fact]
    public async Task UpdatesTokenCache_OnSuccess()
    {
        // Arrange
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

        // Act
        await service.ExchangeAuthorizationCodeAsync("code", "verifier", CancellationToken.None);

        // Assert
        var tokenCache = new FitbitTokenCache(new HttpContextAccessor { HttpContext = httpContext });
        tokenCache.AccessToken.ShouldBe("access");
        tokenCache.RefreshToken.ShouldBe("refresh");
        tokenCache.ExpiresAtUtc.ShouldNotBeNull();
    }
}