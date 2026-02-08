using FakeItEasy;
using Microsoft.Extensions.Logging;
using Shouldly;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.FileUploadsApiTests;

public class EmptyPayload
{
    [Fact]
    public async Task WhenResponseEmpty_Throws()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("null", Encoding.UTF8, "application/json") };

            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new FileUploadsApi(new TestHttpClientFactory(client), A.Fake<ILogger<FileUploadsApi>>());

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => api.UploadBundleAsync(new MemoryStream(new byte[] { 1 }), "bundle.zip", null, CancellationToken.None));

        // Assert
        exception.Message.ShouldBe("Empty upload response payload");
    }
}