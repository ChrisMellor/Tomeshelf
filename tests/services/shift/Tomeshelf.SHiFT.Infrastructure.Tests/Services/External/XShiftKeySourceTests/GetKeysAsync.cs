using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.SHiFT.Application;
using Tomeshelf.SHiFT.Infrastructure.Services.External;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.External.XShiftKeySourceTests;

public class GetKeysAsync
{
    [Fact]
    public async Task ReturnsEmpty_WhenDisabled()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = false
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new RoutingHandler();
        var factory = new StubHttpClientFactory(new HttpClient(handler));
        var tokenProvider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);
        var source = new XShiftKeySource(factory, monitor, tokenProvider, NullLogger<XShiftKeySource>.Instance);

        // Act
        var results = await source.GetKeysAsync(DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        results.ShouldBeEmpty();
        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReturnsEmpty_WhenUsernamesMissing()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = true,
                BearerToken = "token",
                Usernames = new List<string>()
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new RoutingHandler();
        var factory = new StubHttpClientFactory(new HttpClient(handler));
        var tokenProvider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);
        var source = new XShiftKeySource(factory, monitor, tokenProvider, NullLogger<XShiftKeySource>.Instance);

        // Act
        var results = await source.GetKeysAsync(DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        results.ShouldBeEmpty();
        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReturnsEmpty_WhenBearerTokenUnavailable()
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
                OAuthTokenEndpoint = "https://auth.example.test/token",
                Usernames = new List<string> { "Gearbox" }
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new RoutingHandler();
        var factory = new StubHttpClientFactory(new HttpClient(handler));
        var tokenProvider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);
        var source = new XShiftKeySource(factory, monitor, tokenProvider, NullLogger<XShiftKeySource>.Instance);

        // Act
        var results = await source.GetKeysAsync(DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        results.ShouldBeEmpty();
        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReturnsEmpty_WhenUserLookupFails()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = true,
                BearerToken = "token",
                Usernames = new List<string> { "Gearbox" }
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new RoutingHandler { ForceUserLookupFailure = true };
        var factory = new StubHttpClientFactory(new HttpClient(handler));
        var tokenProvider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);
        var source = new XShiftKeySource(factory, monitor, tokenProvider, NullLogger<XShiftKeySource>.Instance);

        // Act
        var results = await source.GetKeysAsync(DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        results.ShouldBeEmpty();
        handler.Requests.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task ReturnsEmpty_WhenUserLookupReturnsNoId()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = true,
                BearerToken = "token",
                Usernames = new List<string> { "Gearbox" }
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new RoutingHandler
        {
            UserLookupResponse = "{ \"data\": { } }"
        };
        var factory = new StubHttpClientFactory(new HttpClient(handler));
        var tokenProvider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);
        var source = new XShiftKeySource(factory, monitor, tokenProvider, NullLogger<XShiftKeySource>.Instance);

        // Act
        var results = await source.GetKeysAsync(DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        results.ShouldBeEmpty();
        handler.Requests.Count.ShouldBe(1);
        handler.Requests[0].RequestUri!.AbsolutePath.ShouldContain("/users/by/username/");
    }

    [Fact]
    public async Task ExtractsCodes_FromTweets()
    {
        // Arrange
        var sinceUtc = new DateTimeOffset(2025, 01, 01, 00, 00, 00, TimeSpan.Zero);
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = true,
                BearerToken = "token",
                Usernames = new List<string> { " Gearbox " },
                ExcludeReplies = true,
                ExcludeRetweets = false
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new RoutingHandler
        {
            UserLookupResponse = """
                                 { "data": { "id": "user-123" } }
                                 """,
            TweetsResponse = """
                             {
                               "data": [
                                 { "id": "1", "text": "old", "created_at": "2024-12-31T23:00:00.000Z" },
                                 { "id": "2", "text": "code: abcde-fghij-klmno-pqrst-uvwxy", "created_at": "2025-01-01T01:00:00.000Z" }
                               ]
                             }
                             """
        };
        var factory = new StubHttpClientFactory(new HttpClient(handler));
        var tokenProvider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);
        var source = new XShiftKeySource(factory, monitor, tokenProvider, NullLogger<XShiftKeySource>.Instance);

        // Act
        var results = await source.GetKeysAsync(sinceUtc, CancellationToken.None);

        // Assert
        results.ShouldHaveSingleItem();
        results[0].Code.ShouldBe("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY");
        results[0].Source.ShouldBe("x:Gearbox");
        results[0].PublishedUtc.ShouldBe(new DateTimeOffset(2025, 01, 01, 01, 00, 00, TimeSpan.Zero));

        handler.LastTweetRequest.ShouldNotBeNull();
        handler.LastTweetRequest!.RequestUri!.Query.ShouldContain("exclude=replies");
        handler.LastTweetRequest.Headers.Authorization.ShouldNotBeNull();
        handler.LastTweetRequest.Headers.Authorization!.Scheme.ShouldBe("Bearer");
        handler.LastTweetRequest.Headers.Authorization!.Parameter.ShouldBe("token");
    }

    [Fact]
    public async Task ReturnsEmpty_WhenTweetFetchFails()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = true,
                BearerToken = "token",
                Usernames = new List<string> { "Gearbox" }
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new RoutingHandler { ForceTweetFailure = true };
        var factory = new StubHttpClientFactory(new HttpClient(handler));
        var tokenProvider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);
        var source = new XShiftKeySource(factory, monitor, tokenProvider, NullLogger<XShiftKeySource>.Instance);

        // Act
        var results = await source.GetKeysAsync(DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        results.ShouldBeEmpty();
        handler.Requests.Count.ShouldBe(2);
        handler.Requests.Any(request => request.RequestUri!.AbsolutePath.Contains("/tweets", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public async Task Paginates_WhenNextTokenProvided()
    {
        // Arrange
        var sinceUtc = new DateTimeOffset(2025, 01, 01, 00, 00, 00, TimeSpan.Zero);
        var options = new ShiftKeyScannerOptions
        {
            X = new ShiftKeyScannerOptions.XSourceOptions
            {
                Enabled = true,
                BearerToken = "token",
                Usernames = new List<string> { "Gearbox" },
                MaxPages = 2,
                MaxResultsPerPage = 250,
                ExcludeReplies = true,
                ExcludeRetweets = true
            }
        };
        var monitor = new TestOptionsMonitor<ShiftKeyScannerOptions>(options);
        var handler = new RoutingHandler
        {
            UserLookupResponse = "{ \"data\": { \"id\": \"user-123\" } }"
        };
        handler.TweetResponses.Enqueue("""
                                       {
                                         "data": [
                                           { "id": "1", "text": "code: ABCDE-FGHIJ-KLMNO-PQRST-UVWXY", "created_at": "2025-01-01T01:00:00.000Z" }
                                         ],
                                         "meta": { "next_token": "next-1" }
                                       }
                                       """);
        handler.TweetResponses.Enqueue("""
                                       {
                                         "data": [
                                           { "id": "2", "text": "code: 11111-22222-33333-44444-55555", "created_at": "2025-01-01T02:00:00.000Z" }
                                         ]
                                       }
                                       """);
        var factory = new StubHttpClientFactory(new HttpClient(handler));
        var tokenProvider = new XAppOnlyTokenProvider(factory, monitor, NullLogger<XAppOnlyTokenProvider>.Instance);
        var source = new XShiftKeySource(factory, monitor, tokenProvider, NullLogger<XShiftKeySource>.Instance);

        // Act
        var results = await source.GetKeysAsync(sinceUtc, CancellationToken.None);

        // Assert
        results.Count.ShouldBe(2);
        results[0].Code.ShouldBe("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY");
        results[1].Code.ShouldBe("11111-22222-33333-44444-55555");

        var tweetRequests = handler.Requests
                                   .Where(request => request.RequestUri!.AbsolutePath.Contains("/tweets", StringComparison.OrdinalIgnoreCase))
                                   .ToList();
        tweetRequests.Count.ShouldBe(2);
        tweetRequests[0].RequestUri!.Query.ShouldContain("max_results=100");
        tweetRequests[0].RequestUri!.Query.ShouldContain("exclude=replies%2Cretweets");
        tweetRequests[0].RequestUri!.Query.ShouldContain("start_time=2025-01-01T00%3A00%3A00.000Z");
        tweetRequests[1].RequestUri!.Query.ShouldContain("pagination_token=next-1");
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public StubHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name)
        {
            return _client;
        }
    }

    private sealed class RoutingHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

        public HttpRequestMessage? LastTweetRequest { get; private set; }

        public string UserLookupResponse { get; set; } = "{ \"data\": { \"id\": \"user-1\" } }";

        public string TweetsResponse { get; set; } = "{ \"data\": [] }";

        public bool ForceUserLookupFailure { get; set; }

        public bool ForceTweetFailure { get; set; }

        public Queue<string> TweetResponses { get; } = new Queue<string>();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;

            if (path.Contains("/users/by/username/", StringComparison.OrdinalIgnoreCase))
            {
                if (ForceUserLookupFailure)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(UserLookupResponse, Encoding.UTF8, "application/json")
                });
            }

            if (path.Contains("/tweets", StringComparison.OrdinalIgnoreCase))
            {
                LastTweetRequest = request;

                if (ForceTweetFailure)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                }

                var payload = TweetResponses.Count > 0
                    ? TweetResponses.Dequeue()
                    : TweetsResponse;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
