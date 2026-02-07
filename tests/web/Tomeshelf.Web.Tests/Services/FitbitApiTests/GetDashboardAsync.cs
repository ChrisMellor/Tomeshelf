using FakeItEasy;
using Microsoft.Extensions.Logging;
using Shouldly;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Fitness;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.FitbitApiTests;

public class GetDashboardAsync
{
    [Fact]
    public async Task BuildsQueryString()
    {
        string? path = null;
        var payload = new FitbitDashboardModel { Date = "2020-01-01" };
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var handler = new StubHttpMessageHandler((request, _) =>
        {
            path = request.RequestUri!.PathAndQuery;
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new FitbitApi(new TestHttpClientFactory(client), A.Fake<ILogger<FitbitApi>>());

        await api.GetDashboardAsync("2020-01-01", true, "https://return.example/?a=1", CancellationToken.None);

        path.ShouldBe("/api/Fitbit/Dashboard?date=2020-01-01&refresh=true&returnUrl=https%3A%2F%2Freturn.example%2F%3Fa%3D1");
    }

    [Fact]
    public async Task WhenUnauthorizedWithoutLocation_UsesDefaultRedirect()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new FitbitApi(new TestHttpClientFactory(client), A.Fake<ILogger<FitbitApi>>());

        var exception = await Should.ThrowAsync<FitbitAuthorizationRequiredException>(() => api.GetDashboardAsync("2020-01-01", false, "https://return.example", CancellationToken.None));

        exception.Location.ShouldBe(new Uri("https://example.test/fitness"));
    }
}