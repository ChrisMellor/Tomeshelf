using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Models.Bundles;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Services.FileUploadsApiTests;

public class UploadBundleAsync
{
    [Fact]
    public async Task IncludesCredentialsInMultipartContent()
    {
        // Arrange
        var parts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? fileName = null;
        var responsePayload = new BundleUploadResultModel { BundlesProcessed = 1 };
        var json = JsonSerializer.Serialize(responsePayload, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            if (request.Content is MultipartFormDataContent form)
            {
                foreach (var part in form)
                {
                    var name = part.Headers.ContentDisposition?.Name?.Trim('"') ?? string.Empty;
                    var partFileName = part.Headers.ContentDisposition?.FileName?.Trim('"');
                    if (!string.IsNullOrWhiteSpace(partFileName))
                    {
                        fileName = partFileName;
                    }

                    if (part is StringContent)
                    {
                        parts[name] = await part.ReadAsStringAsync(cancellationToken);
                    }
                }
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            return response;
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new FileUploadsApi(new TestHttpClientFactory(client), A.Fake<ILogger<FileUploadsApi>>());

        await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var auth = new GoogleDriveAuthModel
        {
            ClientId = "client",
            ClientSecret = "secret",
            RefreshToken = "refresh",
            UserEmail = "user@example.com"
        };

        // Act
        var result = await api.UploadBundleAsync(stream, "bundle.zip", auth, CancellationToken.None);

        // Assert
        result.BundlesProcessed.ShouldBe(1);
        fileName.ShouldBe("bundle.zip");
        parts["credentials.clientId"].ShouldBe("client");
        parts["credentials.clientSecret"].ShouldBe("secret");
        parts["credentials.refreshToken"].ShouldBe("refresh");
        parts["credentials.userEmail"].ShouldBe("user@example.com");
    }

    [Fact]
    public async Task WhenAuthMissing_DoesNotIncludeCredentialFields()
    {
        // Arrange
        var parts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var responsePayload = new BundleUploadResultModel { BundlesProcessed = 1 };
        var json = JsonSerializer.Serialize(responsePayload, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            if (request.Content is MultipartFormDataContent form)
            {
                foreach (var part in form)
                {
                    if (part is StringContent)
                    {
                        var name = part.Headers.ContentDisposition?.Name?.Trim('"') ?? string.Empty;
                        parts[name] = await part.ReadAsStringAsync(cancellationToken);
                    }
                }
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            return response;
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new FileUploadsApi(new TestHttpClientFactory(client), A.Fake<ILogger<FileUploadsApi>>());

        await using var stream = new MemoryStream(new byte[] { 4, 5, 6 });

        // Act
        await api.UploadBundleAsync(stream, "bundle.zip", null, CancellationToken.None);

        // Assert
        parts.ContainsKey("credentials.clientId").ShouldBeFalse();
        parts.ContainsKey("credentials.clientSecret").ShouldBeFalse();
        parts.ContainsKey("credentials.refreshToken").ShouldBeFalse();
        parts.ContainsKey("credentials.userEmail").ShouldBeFalse();
    }

    [Fact]
    public async Task WhenStreamNull_Throws()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var api = new FileUploadsApi(new TestHttpClientFactory(client), A.Fake<ILogger<FileUploadsApi>>());

        // Act
        // Act
        await Should.ThrowAsync<ArgumentNullException>(() => api.UploadBundleAsync(null!, "bundle.zip", null, CancellationToken.None));
    }
}
