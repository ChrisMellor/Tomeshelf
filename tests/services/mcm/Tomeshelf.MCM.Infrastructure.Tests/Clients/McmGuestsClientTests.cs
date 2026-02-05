using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.MCM.Infrastructure.Clients;

namespace Tomeshelf.MCM.Infrastructure.Tests.Clients;

public class McmGuestsClientTests
{
    [Fact]
    public async Task FetchGuestsAsync_ReturnsMappedGuests()
    {
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

        var guests = await subject.FetchGuestsAsync("mcm spring", CancellationToken.None);

        factory.LastName
               .Should()
               .Be(McmGuestsClient.HttpClientName);
        handler.RequestMessage
               .Should()
               .NotBeNull();
        handler.RequestMessage!.Method
               .Should()
               .Be(HttpMethod.Post);
        handler.RequestMessage
               .RequestUri
               .Should()
               .Be(new Uri("https://example.test/api/people?key=mcm%20spring"));

        handler.RequestBody
               .Should()
               .Be("{}");
        handler.RequestContentType
               .Should()
               .Be("application/json");

        guests.Should()
              .HaveCount(2);
        guests[0]
           .Name
           .Should()
           .Be("Ada Lovelace");
        guests[0]
           .Description
           .Should()
           .Be("Bio");
        guests[0]
           .ProfileUrl
           .Should()
           .Be("https://imdb.example/ada");
        guests[0]
           .ImageUrl
           .Should()
           .Be("https://images.example/ada.png");

        guests[1]
           .Name
           .Should()
           .Be("Solo");
        guests[1]
           .Description
           .Should()
           .Be("Known for");
        guests[1]
           .ProfileUrl
           .Should()
           .BeEmpty();
        guests[1]
           .ImageUrl
           .Should()
           .BeEmpty();
    }

    [Fact]
    public async Task FetchGuestsAsync_Throws_WhenPayloadIsEmpty()
    {
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty, Encoding.UTF8, "application/json") });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var subject = new McmGuestsClient(new StubHttpClientFactory(client), NullLogger<McmGuestsClient>.Instance);

        Func<Task> act = () => subject.FetchGuestsAsync("event-1", CancellationToken.None);

        var exception = await act.Should()
                                 .ThrowAsync<InvalidOperationException>();
        exception.WithMessage("MCM API returned an empty payload.");
    }

    [Fact]
    public async Task FetchGuestsAsync_Throws_WhenPeopleMissing()
    {
        var payload = """
                      {
                        "event_id":"event-1",
                        "event_name":"Test Event"
                      }
                      """;
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload, Encoding.UTF8, "application/json") });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var subject = new McmGuestsClient(new StubHttpClientFactory(client), NullLogger<McmGuestsClient>.Instance);

        Func<Task> act = () => subject.FetchGuestsAsync("event-1", CancellationToken.None);

        var exception = await act.Should()
                                 .ThrowAsync<InvalidOperationException>();
        exception.WithMessage("MCM API payload did not include people.");
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