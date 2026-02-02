using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Models.Fitness;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.FitbitApiTests;

public class GetOverviewAsync
{
    [Fact]
    public async Task WhenRedirect_ReturnsAuthorizationRequiredException()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Redirect)
            {
                Headers = { Location = new Uri("/auth", UriKind.Relative) }
            };
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new FitbitApi(new TestHttpClientFactory(client), A.Fake<ILogger<FitbitApi>>());

        // Act
        var action = () => api.GetOverviewAsync("2020-01-01", false, "https://return", CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<FitbitAuthorizationRequiredException>();
        exception.Which.Location.Should().Be(new Uri("https://example.test/auth"));
    }

    [Fact]
    public async Task WhenNotFound_ReturnsNull()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new FitbitApi(new TestHttpClientFactory(client), A.Fake<ILogger<FitbitApi>>());

        // Act
        var result = await api.GetOverviewAsync("2020-01-01", false, "https://return", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task WhenTooManyRequestsWithEmptyBody_ThrowsRateLimitMessage()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
            };
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new FitbitApi(new TestHttpClientFactory(client), A.Fake<ILogger<FitbitApi>>());

        // Act
        var action = () => api.GetOverviewAsync("2020-01-01", false, "https://return", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<FitbitBackendUnavailableException>()
            .WithMessage("Fitbit rate limit reached. Please wait a moment and try again.");
    }

    [Fact]
    public async Task WhenSuccess_DeserializesPayload()
    {
        // Arrange
        var payload = new FitbitOverviewModel
        {
            Daily = new FitbitDashboardModel { Date = "2020-01-01" }
        };
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new FitbitApi(new TestHttpClientFactory(client), A.Fake<ILogger<FitbitApi>>());

        // Act
        var result = await api.GetOverviewAsync("2020-01-01", false, "https://return", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Daily.Date.Should().Be("2020-01-01");
    }
}
