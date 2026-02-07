using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using System.Net;
using System.Text;
using Tomeshelf.HumbleBundle.Infrastructure.Bundles;

namespace Tomeshelf.HumbleBundle.Infrastructure.Tests.Bundles.HumbleBundleScraperTests;

public class ScrapeAsync
{
    [Fact]
    public async Task ParsesBundlesFromHtmlPayload()
    {
        var json = """
                   {"userOptions":{},"data":{"books":{"mosaic":[{"products":[
                     {"machine_name":"bundle-one","tile_stamp":"Bundle","tile_name":"Bundle One","tile_short_name":"One","short_marketing_blurb":"Deal {with} braces","product_url":"/bundle-one","tile_image":"https://img.test/tile.png","tile_logo":"https://img.test/logo.png","high_res_tile_image":"https://img.test/hero.png","start_date|datetime":"2025-01-01T00:00:00Z","end_date|datetime":"2025-01-10T00:00:00Z"},
                     {"machine_name":"bundle-two","product_url":"https://www.humblebundle.com/bundle-two","tile_image":"https://img.test/tile2.png"},
                     {"tile_name":"missing-machine"}
                   ]}]}}}
                   """;

        var html = $"<html><body>{json}</body></html>";
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(html, Encoding.UTF8, "text/html") });
        var client = new HttpClient(handler);
        var factory = new StubHttpClientFactory(client);
        var scraper = new HumbleBundleScraper(factory, NullLogger<HumbleBundleScraper>.Instance);
        var before = DateTimeOffset.UtcNow;

        var bundles = await scraper.ScrapeAsync(CancellationToken.None);

        var after = DateTimeOffset.UtcNow;
        factory.LastName.ShouldBe(HumbleBundleScraper.HttpClientName);
        handler.RequestMessage.ShouldNotBeNull();
        handler.RequestMessage!.RequestUri.ShouldBe(new Uri("https://www.humblebundle.com/bundles"));
        handler.RequestMessage.Method.ShouldBe(HttpMethod.Get);

        bundles.Count.ShouldBe(2);

        var first = bundles[0];
        first.MachineName.ShouldBe("bundle-one");
        first.Category.ShouldBe("books");
        first.Stamp.ShouldBe("Bundle");
        first.Title.ShouldBe("Bundle One");
        first.ShortName.ShouldBe("One");
        first.Url.ShouldBe("https://www.humblebundle.com/bundle-one");
        first.TileImageUrl.ShouldBe("https://img.test/tile.png");
        first.TileLogoUrl.ShouldBe("https://img.test/logo.png");
        first.HeroImageUrl.ShouldBe("https://img.test/hero.png");
        first.ShortDescription.ShouldBe("Deal {with} braces");
        first.StartsAt.ShouldBe(DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
        first.EndsAt.ShouldBe(DateTimeOffset.Parse("2025-01-10T00:00:00Z"));
        first.ObservedUtc.ShouldBeInRange(before, after);

        var second = bundles[1];
        second.MachineName.ShouldBe("bundle-two");
        second.Title.ShouldBe("bundle-two");
        second.Url.ShouldBe("https://www.humblebundle.com/bundle-two");
        second.TileImageUrl.ShouldBe("https://img.test/tile2.png");
        second.HeroImageUrl.ShouldBe("https://img.test/tile2.png");
        second.Stamp.ShouldBe(string.Empty);
        second.ShortName.ShouldBe(string.Empty);
        second.TileLogoUrl.ShouldBe(string.Empty);
        second.ShortDescription.ShouldBe(string.Empty);
        second.StartsAt.ShouldBeNull();
        second.EndsAt.ShouldBeNull();
    }

    [Fact]
    public async Task ReturnsEmptyList_WhenNoBundleDataIsPresent()
    {
        var json = """
                   {"userOptions":{},"data":{}}
                   """;
        var html = $"<html><body>{json}</body></html>";
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(html, Encoding.UTF8, "text/html") });
        var scraper = new HumbleBundleScraper(new StubHttpClientFactory(new HttpClient(handler)), NullLogger<HumbleBundleScraper>.Instance);

        var bundles = await scraper.ScrapeAsync(CancellationToken.None);

        bundles.ShouldBeEmpty();
    }

    [Fact]
    public async Task Throws_WhenJsonPayloadIsMissing()
    {
        var html = "<html><body>No payload</body></html>";
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(html, Encoding.UTF8, "text/html") });
        var scraper = new HumbleBundleScraper(new StubHttpClientFactory(new HttpClient(handler)), NullLogger<HumbleBundleScraper>.Instance);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => scraper.ScrapeAsync(CancellationToken.None));

        exception.Message.ShouldBe("Unable to locate Humble Bundle JSON payload in the HTML response.");
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

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestMessage = request;

            return Task.FromResult(_response);
        }
    }
}