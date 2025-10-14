using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Services.GuestsApiTests;

public class GuestsApiTests
{
    [Fact]
    public async Task GetComicConGuestsByCityAsync_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ invalid", Encoding.UTF8, "application/json") });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var api = new GuestsApi(http, NullLogger<GuestsApi>.Instance);

        // Act
        Func<Task> act = async () => await api.GetComicConGuestsByCityAsync("London", TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
                 .ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task GetComicConGuestsByCityAsync_EmptyBody_ThrowsJsonException()
    {
        // Arrange
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("", Encoding.UTF8, "application/json") });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var api = new GuestsApi(http, NullLogger<GuestsApi>.Instance);

        // Act
        Func<Task> act = async () => await api.GetComicConGuestsByCityAsync("London", TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
                 .ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task GetComicConGuestsByCityAsync_ParsesResponse()
    {
        // Arrange
        var handler = new StubHandler(request =>
        {
            var json = "{" + "\"city\":\"London\",\"total\":1,\"groups\":[{" + "\"createdDate\":\"2025-01-01T00:00:00Z\",\"items\":[{" + "\"id\":\"1\",\"first_name\":\"Ada\",\"last_name\":\"Lovelace\",\"images\":[]" + "}]}]}";

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var api = new GuestsApi(http, NullLogger<GuestsApi>.Instance);

        // Act
        var (groups, total) = await api.GetComicConGuestsByCityAsync("London", TestContext.Current.CancellationToken);

        // Assert
        total.Should()
             .Be(1);
        groups.Should()
              .ContainSingle();
        var firstPerson = groups[0]
               .Items[0];
        firstPerson.Should()
                   .BeEquivalentTo(new
                    {
                            FirstName = "Ada",
                            LastName = "Lovelace"
                    });
    }

    [Fact]
    public async Task GetComicConGuestsByCityAsync_ThrowsOnNonSuccess()
    {
        // Arrange
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.BadGateway));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var api = new GuestsApi(http, NullLogger<GuestsApi>.Instance);

        // Act
        Func<Task> act = async () => await api.GetComicConGuestsByCityAsync("London", TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
                 .ThrowAsync<HttpRequestException>();
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }
}