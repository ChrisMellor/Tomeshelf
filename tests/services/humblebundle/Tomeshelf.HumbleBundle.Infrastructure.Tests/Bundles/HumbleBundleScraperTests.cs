using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.HumbleBundle.Infrastructure.Bundles;

namespace Tomeshelf.HumbleBundle.Infrastructure.Tests.Bundles;

public class HumbleBundleScraperTests
{
    [Fact]
    public async Task ScrapeAsync_ParsesBundlesFromHtmlPayload()
    {
        var json = """
{"userOptions":{},"data":{"books":{"mosaic":[{"products":[
  {"machine_name":"bundle-one","tile_stamp":"Bundle","tile_name":"Bundle One","tile_short_name":"One","short_marketing_blurb":"Deal {with} braces","product_url":"/bundle-one","tile_image":"https://img.test/tile.png","tile_logo":"https://img.test/logo.png","high_res_tile_image":"https://img.test/hero.png","start_date|datetime":"2025-01-01T00:00:00Z","end_date|datetime":"2025-01-10T00:00:00Z"},
  {"machine_name":"bundle-two","product_url":"https://www.humblebundle.com/bundle-two","tile_image":"https://img.test/tile2.png"},
  {"tile_name":"missing-machine"}
]}]}}}
""";

        var html = $"<html><body>{json}</body></html>";
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html, Encoding.UTF8, "text/html")
        });
        var client = new HttpClient(handler);
        var factory = new StubHttpClientFactory(client);
        var scraper = new HumbleBundleScraper(factory, NullLogger<HumbleBundleScraper>.Instance);
        var before = DateTimeOffset.UtcNow;

        var bundles = await scraper.ScrapeAsync(CancellationToken.None);

        var after = DateTimeOffset.UtcNow;

        factory.LastName.Should().Be(HumbleBundleScraper.HttpClientName);
        handler.RequestMessage.Should().NotBeNull();
        handler.RequestMessage!.RequestUri.Should().Be(new Uri("https://www.humblebundle.com/bundles"));
        handler.RequestMessage.Method.Should().Be(HttpMethod.Get);

        bundles.Should().HaveCount(2);

        var first = bundles[0];
        first.MachineName.Should().Be("bundle-one");
        first.Category.Should().Be("books");
        first.Stamp.Should().Be("Bundle");
        first.Title.Should().Be("Bundle One");
        first.ShortName.Should().Be("One");
        first.Url.Should().Be("https://www.humblebundle.com/bundle-one");
        first.TileImageUrl.Should().Be("https://img.test/tile.png");
        first.TileLogoUrl.Should().Be("https://img.test/logo.png");
        first.HeroImageUrl.Should().Be("https://img.test/hero.png");
        first.ShortDescription.Should().Be("Deal {with} braces");
        first.StartsAt.Should().Be(DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
        first.EndsAt.Should().Be(DateTimeOffset.Parse("2025-01-10T00:00:00Z"));
        first.ObservedUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

        var second = bundles[1];
        second.MachineName.Should().Be("bundle-two");
        second.Title.Should().Be("bundle-two");
        second.Url.Should().Be("https://www.humblebundle.com/bundle-two");
        second.TileImageUrl.Should().Be("https://img.test/tile2.png");
        second.HeroImageUrl.Should().Be("https://img.test/tile2.png");
        second.Stamp.Should().BeEmpty();
        second.ShortName.Should().BeEmpty();
        second.TileLogoUrl.Should().BeEmpty();
        second.ShortDescription.Should().BeEmpty();
        second.StartsAt.Should().BeNull();
        second.EndsAt.Should().BeNull();
    }

    [Fact]
    public async Task ScrapeAsync_Throws_WhenJsonPayloadIsMissing()
    {
        var html = "<html><body>No payload</body></html>";
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html, Encoding.UTF8, "text/html")
        });
        var scraper = new HumbleBundleScraper(new StubHttpClientFactory(new HttpClient(handler)), NullLogger<HumbleBundleScraper>.Instance);

        Func<Task> act = () => scraper.ScrapeAsync(CancellationToken.None);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.WithMessage("Unable to locate Humble Bundle JSON payload in the HTML response.");
    }

    [Fact]
    public async Task ScrapeAsync_ReturnsEmptyList_WhenNoBundleDataIsPresent()
    {
        var json = """
{"userOptions":{},"data":{}}
""";
        var html = $"<html><body>{json}</body></html>";
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html, Encoding.UTF8, "text/html")
        });
        var scraper = new HumbleBundleScraper(new StubHttpClientFactory(new HttpClient(handler)), NullLogger<HumbleBundleScraper>.Instance);

        var bundles = await scraper.ScrapeAsync(CancellationToken.None);

        bundles.Should().BeEmpty();
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
