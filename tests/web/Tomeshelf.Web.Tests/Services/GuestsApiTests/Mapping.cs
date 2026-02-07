using System;
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

public class Mapping
{
    [Fact]
    public async Task MapsGuestFieldsAndImages()
    {
        var payload = new
        {
            total = 1,
            page = 1,
            pageSize = 200,
            items = new[]
            {
                new McmGuestDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Jane Doe",
                    Description = "Bio",
                    ProfileUrl = "https://example.test/profile",
                    ImageUrl = " https://example.test/image.jpg ",
                    AddedAt = new DateTimeOffset(2020, 1, 2, 10, 0, 0, TimeSpan.Zero),
                    RemovedAt = new DateTimeOffset(2020, 1, 3, 9, 0, 0, TimeSpan.Zero),
                    IsDeleted = true
                }
            }
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var cache = new MemoryCache(new MemoryCacheOptions());
        var api = new GuestsApi(new TestHttpClientFactory(client), A.Fake<ILogger<GuestsApi>>(), cache);

        var result = await api.GetComicConGuestsByEventResultAsync("mcm-2026", CancellationToken.None);

        var person = result.Groups[0]
                           .Items[0];
        person.FirstName.ShouldBe("Jane");
        person.LastName.ShouldBe("Doe");
        person.PubliclyVisible.ShouldBeFalse();
        person.ProfileUrl.ShouldBe("https://example.test/profile");
        var image = person.Images.ShouldHaveSingleItem();
        image.Big.ShouldBe("https://example.test/image.jpg");
        person.RemovedAt.ShouldBe("2020-01-03T09:00:00.0000000+00:00");
    }
}