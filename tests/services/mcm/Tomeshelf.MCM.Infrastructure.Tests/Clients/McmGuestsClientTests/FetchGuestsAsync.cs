using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.MCM.Infrastructure.Clients;
using Tomeshelf.MCM.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.MCM.Infrastructure.Tests.Clients.McmGuestsClientTests;

public class FetchGuestsAsync
{
    [Fact]
    public async Task ReturnsMappedGuests()
    {
        // Arrange
        var payload = """
                      {
                        "event_id":"event-1",
                        "event_name":"Test Event",
                        "event_slug":"test",
                        "people":[
                          null,
                          {
                            "first_name":"  Ada  ",
                            "last_name":" Lovelace ",
                            "bio":"Bio",
                            "profile_url":"   ",
                            "imdb":"https://imdb.example/ada",
                            "images":[
                              { "big":"", "med":"", "small":"https://images.example/ada.png" }
                            ]
                          },
                          {
                            "alt_name":"  Solo  ",
                            "bio":"   ",
                            "known_for":"Known for",
                            "images":[]
                          },
                          {
                            "first_name":" ",
                            "last_name":" ",
                            "alt_name":"   "
                          }
                        ]
                      }
                      """;

        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload, Encoding.UTF8, "application/json") });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var factory = new StubHttpClientFactory(client);
        var subject = new McmGuestsClient(factory, NullLogger<McmGuestsClient>.Instance);

        // Act
        var guests = await subject.FetchGuestsAsync("mcm spring", CancellationToken.None);

        // Assert
        factory.LastName.ShouldBe(McmGuestsClient.HttpClientName);
        handler.RequestMessage.ShouldNotBeNull();
        handler.RequestMessage!.Method.ShouldBe(HttpMethod.Post);
        handler.RequestMessage.RequestUri.ShouldBe(new Uri("https://example.test/api/people?key=mcm%20spring"));
        handler.RequestBody.ShouldBe("{}");
        handler.RequestContentType.ShouldBe("application/json");

        guests.Count.ShouldBe(2);
        guests[0].Name.ShouldBe("Ada Lovelace");
        guests[0].Description.ShouldBe("Bio");
        guests[0].ProfileUrl.ShouldBe("https://imdb.example/ada");
        guests[0].ImageUrl.ShouldBe("https://images.example/ada.png");

        guests[1].Name.ShouldBe("Solo");
        guests[1].Description.ShouldBe("Known for");
        guests[1].ProfileUrl.ShouldBe(string.Empty);
        guests[1].ImageUrl.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task Throws_WhenPayloadIsEmpty()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty, Encoding.UTF8, "application/json") });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var subject = new McmGuestsClient(new StubHttpClientFactory(client), NullLogger<McmGuestsClient>.Instance);

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => subject.FetchGuestsAsync("event-1", CancellationToken.None));

        // Assert
        exception.Message.ShouldBe("MCM API returned an empty payload.");
    }

    [Fact]
    public async Task Throws_WhenPeopleMissing()
    {
        // Arrange
        var payload = """
                      {
                        "event_id":"event-1",
                        "event_name":"Test Event"
                      }
                      """;
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload, Encoding.UTF8, "application/json") });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var subject = new McmGuestsClient(new StubHttpClientFactory(client), NullLogger<McmGuestsClient>.Instance);

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => subject.FetchGuestsAsync("event-1", CancellationToken.None));

        // Assert
        exception.Message.ShouldBe("MCM API payload did not include people.");
    }

    [Fact]
    public async Task Throws_WhenApiReturnsError()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var subject = new McmGuestsClient(new StubHttpClientFactory(client), NullLogger<McmGuestsClient>.Instance);

        // Act
        await Should.ThrowAsync<HttpRequestException>(() => subject.FetchGuestsAsync("test-event", CancellationToken.None));

        // Assert
        handler.RequestMessage.ShouldNotBeNull();
    }

    [Fact]
    public async Task LogsWarning_WhenEventIdContainsSpecialCharacters()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var factory = new StubHttpClientFactory(client);
        var logger = new TestLogger<McmGuestsClient>();
        var subject = new McmGuestsClient(factory, logger);
        var eventIdWithSpecialChars = "test-event\r\nwith\rchars";
        var cleanedEventId = "test-eventwithchars";

        // Act
        await Should.ThrowAsync<HttpRequestException>(() => subject.FetchGuestsAsync(eventIdWithSpecialChars, CancellationToken.None));

        // Assert
        logger.LogEntries.ShouldContain(entry =>
            entry.LogLevel == LogLevel.Warning &&
            entry.EventId.Id == 0 &&
            entry.Message == $"MCM API returned {HttpStatusCode.InternalServerError} for event {cleanedEventId}.");
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public StubHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public string? LastName { get; private set; }

        public HttpClient CreateClient(string name)
        {
            LastName = name;

            return _client;
        }
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StubHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public HttpRequestMessage? RequestMessage { get; private set; }

        public string? RequestBody { get; private set; }

        public string? RequestContentType { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestMessage = request;
            if (request.Content != null)
            {
                RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                RequestContentType = request.Content.Headers.ContentType?.MediaType;
            }

            return _response;
        }
    }
}
