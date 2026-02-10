using FakeItEasy;
using Microsoft.Extensions.Logging;
using Shouldly;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.FitbitApiTests;

public class Errors
{
    /// <summary>
    ///     Returns message when the service is unavailable.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task WhenServiceUnavailable_ReturnsMessage()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("service down", Encoding.UTF8, "text/plain") };

            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new FitbitApi(new TestHttpClientFactory(client), A.Fake<ILogger<FitbitApi>>());

        // Act
        var exception = await Should.ThrowAsync<FitbitBackendUnavailableException>(() => api.GetOverviewAsync("2020-01-01", false, "https://return", CancellationToken.None));

        // Assert
        exception.Message.ShouldBe("service down");
    }
}