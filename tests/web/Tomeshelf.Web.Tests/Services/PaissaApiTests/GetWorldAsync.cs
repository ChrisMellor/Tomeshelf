using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Models.Paissa;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.PaissaApiTests;

public class GetWorldAsync
{
    [Fact]
    public async Task DeserializesPayload()
    {
        // Arrange
        var world = new PaissaWorldModel(1, "World", DateTimeOffset.UtcNow, new List<PaissaDistrictModel>());
        var json = JsonSerializer.Serialize(world, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new PaissaApi(new TestHttpClientFactory(client), A.Fake<ILogger<PaissaApi>>());

        // Act
        var result = await api.GetWorldAsync(CancellationToken.None);

        // Assert
        result.WorldName.Should().Be("World");
        handler.Requests.Should().ContainSingle();
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/paissa/world");
    }

    [Fact]
    public async Task WhenPayloadEmpty_Throws()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new PaissaApi(new TestHttpClientFactory(client), A.Fake<ILogger<PaissaApi>>());

        // Act
        var action = () => api.GetWorldAsync(CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Empty Paissa payload");
    }
}
