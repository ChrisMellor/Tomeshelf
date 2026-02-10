using FakeItEasy;
using Microsoft.Extensions.Logging;
using Shouldly;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Shift;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.ShiftApiTests;

public class EnumParsing
{
    /// <summary>
    ///     Parses the error code enum from string.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ParsesErrorCodeEnumFromString()
    {
        // Arrange
        var json = "{\"summary\":{\"total\":1,\"succeeded\":0,\"failed\":1},\"results\":[{\"accountId\":1,\"email\":\"user@example.com\",\"service\":\"steam\",\"success\":false,\"errorCode\":\"NetworkError\",\"message\":\"oops\"}]}";

        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new ShiftApi(new TestHttpClientFactory(client), A.Fake<ILogger<ShiftApi>>());

        // Act
        var result = await api.RedeemCodeAsync("ABC", CancellationToken.None);

        // Assert
        result.Results[0]
              .ErrorCode
              .ShouldBe(RedeemErrorCode.NetworkError);
    }
}