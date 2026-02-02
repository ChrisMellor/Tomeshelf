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
using Tomeshelf.Web.Models.Bundles;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.BundlesApiTests;

public class GetBundlesAsync
{
    [Fact]
    public async Task DeserializesResponse()
    {
        // Arrange
        var payload = new List<BundleModel>
        {
            new() { MachineName = "bundle-one", Title = "Bundle One" }
        };
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var handler = new StubHttpMessageHandler((request, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new BundlesApi(new TestHttpClientFactory(client), A.Fake<ILogger<BundlesApi>>());

        // Act
        var result = await api.GetBundlesAsync(true, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].MachineName.Should().Be("bundle-one");
        handler.Requests.Should().ContainSingle();
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/bundles?includeExpired=true");
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
        var api = new BundlesApi(new TestHttpClientFactory(client), A.Fake<ILogger<BundlesApi>>());

        // Act
        var action = () => api.GetBundlesAsync(false, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Empty bundle payload");
    }
}
