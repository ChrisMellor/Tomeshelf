using FakeItEasy;
using Microsoft.Extensions.Logging;
using Shouldly;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Shift;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.ShiftApiTests;

public class RedeemCodeAsync
{
    [Fact]
    public async Task PostsCodeAndDeserializesResponse()
    {
        string? requestBody = null;
        var responsePayload = new RedeemResponseModel(new RedeemSummaryModel(1, 1, 0), new[] { new RedeemResultModel(1, "user@example.com", "steam", true, null, null) });

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

        var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            requestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            var json = JsonSerializer.Serialize(responsePayload, options);
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            return response;
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new ShiftApi(new TestHttpClientFactory(client), A.Fake<ILogger<ShiftApi>>());

        var result = await api.RedeemCodeAsync("ABC", CancellationToken.None);

        result.Summary.Total.ShouldBe(1);
        var request = handler.Requests.ShouldHaveSingleItem();
        request.RequestUri!.PathAndQuery.ShouldBe("/gearbox/redeem");
        requestBody.ShouldContain("\"code\":\"ABC\"");
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
        var api = new ShiftApi(new TestHttpClientFactory(client), A.Fake<ILogger<ShiftApi>>());

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => api.RedeemCodeAsync("ABC", CancellationToken.None));

        exception.Message.ShouldBe("Empty SHiFT payload");
    }
}