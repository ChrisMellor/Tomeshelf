using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Shouldly;
using Tomeshelf.Web.Models.Mcm;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.GuestsApiTests;

public class GetComicConEventsAsync
{
    /// <summary>
    ///     Filters the and caches events.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task FiltersAndCachesEvents()
    {
        // Arrange
        var payload = new List<McmEventConfigModel>
        {
            new()
            {
                Id = "mcm-1",
                Name = "London"
            },
            new()
            {
                Id = "",
                Name = "Missing"
            },
            new()
            {
                Id = "mcm-2",
                Name = "Birmingham"
            }
        };
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var callCount = 0;
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            callCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var cache = new MemoryCache(new MemoryCacheOptions());
        var api = new GuestsApi(new TestHttpClientFactory(client), A.Fake<ILogger<GuestsApi>>(), cache);

        // Act
        var first = await api.GetComicConEventsAsync(CancellationToken.None);
        var second = await api.GetComicConEventsAsync(CancellationToken.None);

        // Assert
        callCount.ShouldBe(1);
        first.Count.ShouldBe(2);
        first[0]
           .Name
           .ShouldBe("Birmingham");
        first[1]
           .Name
           .ShouldBe("London");
        second.Select(item => item.Id)
              .ShouldBe(first.Select(item => item.Id));
        second.Select(item => item.Name)
              .ShouldBe(first.Select(item => item.Name));
    }
}