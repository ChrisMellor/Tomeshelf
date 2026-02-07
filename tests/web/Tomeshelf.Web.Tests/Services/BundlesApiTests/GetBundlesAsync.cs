using FakeItEasy;
using Microsoft.Extensions.Logging;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Bundles;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.BundlesApiTests;

public class GetBundlesAsync
{
    [Fact]
    public async Task DeserializesResponse()
    {
        var payload = new List<BundleModel>
        {
            new()
            {
                MachineName = "bundle-one",
                Title = "Bundle One"
            }
        };
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var handler = new StubHttpMessageHandler((request, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new BundlesApi(new TestHttpClientFactory(client), A.Fake<ILogger<BundlesApi>>());

        var result = await api.GetBundlesAsync(true, CancellationToken.None);

        var bundle = result.ShouldHaveSingleItem();
        bundle.MachineName.ShouldBe("bundle-one");
        var request = handler.Requests.ShouldHaveSingleItem();
        request.RequestUri!.PathAndQuery.ShouldBe("/bundles?includeExpired=true");
    }

    [Fact]
    public async Task WhenPayloadEmpty_Throws()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("null", Encoding.UTF8, "application/json") };

            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new BundlesApi(new TestHttpClientFactory(client), A.Fake<ILogger<BundlesApi>>());

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => api.GetBundlesAsync(false, CancellationToken.None));

        exception.Message.ShouldBe("Empty bundle payload");
    }
}