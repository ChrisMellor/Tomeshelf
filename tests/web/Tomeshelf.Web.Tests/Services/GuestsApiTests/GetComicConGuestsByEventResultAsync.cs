using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Models.Mcm;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.GuestsApiTests;

public class GetComicConGuestsByEventResultAsync
{
    [Fact]
    public async Task PaginatesAndGroupsGuests()
    {
        // Arrange
        var page1 = new
        {
            total = 3,
            page = 1,
            pageSize = 200,
            items = new[]
            {
                new McmGuestDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Zed Alpha",
                    AddedAt = new DateTimeOffset(2020, 1, 2, 10, 0, 0, TimeSpan.Zero)
                },
                new McmGuestDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Adam Beta",
                    AddedAt = new DateTimeOffset(2020, 1, 1, 9, 0, 0, TimeSpan.Zero)
                }
            }
        };

        var page2 = new
        {
            total = 3,
            page = 2,
            pageSize = 200,
            items = new[]
            {
                new McmGuestDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Bob Alpha",
                    AddedAt = new DateTimeOffset(2020, 1, 2, 12, 0, 0, TimeSpan.Zero)
                }
            }
        };

        var handler = new StubHttpMessageHandler((request, _) =>
        {
            var query = request.RequestUri!.Query;
            var json = query.Contains("page=2", StringComparison.Ordinal)
                ? JsonSerializer.Serialize(page2, new JsonSerializerOptions(JsonSerializerDefaults.Web))
                : JsonSerializer.Serialize(page1, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var cache = new MemoryCache(new MemoryCacheOptions());
        var api = new GuestsApi(new TestHttpClientFactory(client), A.Fake<ILogger<GuestsApi>>(), cache);

        // Act
        var result = await api.GetComicConGuestsByEventResultAsync("mcm-2026", CancellationToken.None);

        // Assert
        result.Total.ShouldBe(3);
        result.Groups.Count.ShouldBe(2);
        result.Groups[0].CreatedDate.ShouldBe(new DateTime(2020, 1, 2));
        result.Groups[0].Items.Count.ShouldBe(2);
        result.Groups[0].Items[0].FirstName.ShouldBe("Bob");
        result.Groups[0].Items[0].LastName.ShouldBe("Alpha");
        result.Groups[1].CreatedDate.ShouldBe(new DateTime(2020, 1, 1));
        handler.Requests.Count.ShouldBe(2);
    }
}
