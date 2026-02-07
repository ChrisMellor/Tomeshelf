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
using Tomeshelf.Web.Models.Paissa;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.PaissaApiTests;

public class GetWorldAsync
{
    [Fact]
    public async Task DeserializesPayload()
    {
        var world = new PaissaWorldModel(1, "World", DateTimeOffset.UtcNow, new List<PaissaDistrictModel>());
        var json = JsonSerializer.Serialize(world, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new PaissaApi(new TestHttpClientFactory(client), A.Fake<ILogger<PaissaApi>>());

        var result = await api.GetWorldAsync(CancellationToken.None);

        result.WorldName.ShouldBe("World");
        var request = handler.Requests.ShouldHaveSingleItem();
        request.RequestUri!.PathAndQuery.ShouldBe("/paissa/world");
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
        var api = new PaissaApi(new TestHttpClientFactory(client), A.Fake<ILogger<PaissaApi>>());

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => api.GetWorldAsync(CancellationToken.None));

        exception.Message.ShouldBe("Empty Paissa payload");
    }
}