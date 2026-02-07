using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.SHiFT.Application;
using Tomeshelf.SHiFT.Infrastructure.Services.External;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.External.XAppOnlyTokenProviderTests;

public class GetBearerTokenAsync
{
    [Fact]
    public async Task ReturnsBearerToken_WhenConfigured()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = true,
                BearerToken = "  token-123  "
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new SequenceTokenHandler(new[] { "unused" });
        var factory = new SpyHttpClientFactory(new HttpClient(handler));
        var provider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);

        // Act
        var token = await provider.GetBearerTokenAsync(CancellationToken.None);

        // Assert
        token.ShouldBe("token-123");
        factory.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task ReturnsNull_WhenCredentialsMissing()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = true,
                BearerToken = string.Empty,
                ApiKey = string.Empty,
                ApiSecret = string.Empty,
                OAuthTokenEndpoint = "https://auth.example.test/token"
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new SequenceTokenHandler(new[] { "unused" });
        var factory = new SpyHttpClientFactory(new HttpClient(handler));
        var provider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);

        // Act
        var token = await provider.GetBearerTokenAsync(CancellationToken.None);

        // Assert
        token.ShouldBeNull();
        factory.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task CachesToken_UntilInvalidated()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = true,
                BearerToken = string.Empty,
                ApiKey = "key",
                ApiSecret = "secret",
                OAuthTokenEndpoint = "https://auth.example.test/token",
                TokenCacheMinutes = 10
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new SequenceTokenHandler(new[] { "token-1", "token-2" });
        var factory = new SpyHttpClientFactory(new HttpClient(handler));
        var provider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);

        // Act
        var first = await provider.GetBearerTokenAsync(CancellationToken.None);
        var second = await provider.GetBearerTokenAsync(CancellationToken.None);

        // Assert
        first.ShouldBe("token-1");
        second.ShouldBe("token-1");
        handler.CallCount.ShouldBe(1);
        factory.LastName.ShouldBe(XAppOnlyTokenProvider.HttpClientName);

        // Act
        provider.Invalidate();
        var third = await provider.GetBearerTokenAsync(CancellationToken.None);

        // Assert
        third.ShouldBe("token-2");
        handler.CallCount.ShouldBe(2);
    }

    [Fact]
    public async Task ReturnsNull_WhenOAuthEndpointMissing()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = true,
                ApiKey = "key",
                ApiSecret = "secret",
                OAuthTokenEndpoint = ""
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new SequenceTokenHandler(new[] { "token-1" });
        var factory = new SpyHttpClientFactory(new HttpClient(handler));
        var provider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);

        // Act
        var token = await provider.GetBearerTokenAsync(CancellationToken.None);

        // Assert
        token.ShouldBeNull();
        factory.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task ReturnsNull_WhenResponseFails()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = true,
                ApiKey = "key",
                ApiSecret = "secret",
                OAuthTokenEndpoint = "https://auth.example.test/token"
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new FailureTokenHandler(HttpStatusCode.Unauthorized, "");
        var factory = new SpyHttpClientFactory(new HttpClient(handler));
        var provider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);

        // Act
        var token = await provider.GetBearerTokenAsync(CancellationToken.None);

        // Assert
        token.ShouldBeNull();
        handler.CallCount.ShouldBe(1);
    }

    [Fact]
    public async Task ReturnsNull_WhenTokenMissingFromPayload()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = true,
                ApiKey = "key",
                ApiSecret = "secret",
                OAuthTokenEndpoint = "https://auth.example.test/token"
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new FailureTokenHandler(HttpStatusCode.OK, "{ \"token_type\": \"bearer\" }");
        var factory = new SpyHttpClientFactory(new HttpClient(handler));
        var provider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);

        // Act
        var token = await provider.GetBearerTokenAsync(CancellationToken.None);

        // Assert
        token.ShouldBeNull();
        handler.CallCount.ShouldBe(1);
    }

    private sealed class SpyHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public SpyHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public int CallCount { get; private set; }

        public string? LastName { get; private set; }

        public HttpClient CreateClient(string name)
        {
            CallCount++;
            LastName = name;

            return _client;
        }
    }

    private sealed class SequenceTokenHandler : HttpMessageHandler
    {
        private readonly Queue<string> _tokens;

        public SequenceTokenHandler(IEnumerable<string> tokens)
        {
            _tokens = new Queue<string>(tokens);
        }

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            var token = _tokens.Count > 0 ? _tokens.Dequeue() : "token";
            var payload = "{\"token_type\":\"bearer\",\"access_token\":\"" + token + "\"}";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class FailureTokenHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _payload;

        public FailureTokenHandler(HttpStatusCode status, string payload)
        {
            _status = status;
            _payload = payload;
        }

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;

            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_payload, Encoding.UTF8, "application/json")
            });
        }
    }
}
