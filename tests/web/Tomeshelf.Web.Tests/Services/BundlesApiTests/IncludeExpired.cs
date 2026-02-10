using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Shouldly;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.BundlesApiTests;

public class IncludeExpired
{
    /// <summary>
    ///     Uses the lowercase boolean.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UsesLowercaseBoolean()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]", Encoding.UTF8, "application/json") };

            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new BundlesApi(new TestHttpClientFactory(client), A.Fake<ILogger<BundlesApi>>());

        // Act
        await api.GetBundlesAsync(false, CancellationToken.None);

        // Assert
        var request = handler.Requests.ShouldHaveSingleItem();
        request.RequestUri!.PathAndQuery.ShouldBe("/bundles?includeExpired=false");
    }
}