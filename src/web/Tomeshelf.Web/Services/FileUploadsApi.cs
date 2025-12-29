using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Bundles;

namespace Tomeshelf.Web.Services;

/// <summary>
///     HTTP client for the bundle file uploader API.
/// </summary>
public sealed class FileUploadsApi(HttpClient http, ILogger<FileUploadsApi> logger) : IFileUploadsApi
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public async Task<BundleUploadResultModel> UploadBundleAsync(Stream archiveStream, string fileName, GoogleDriveAuthModel? auth, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archiveStream);

        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(archiveStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

        content.Add(streamContent, "archive", fileName);
        if (auth is not null)
        {
            if (!string.IsNullOrWhiteSpace(auth.ClientId))
            {
                content.Add(new StringContent(auth.ClientId), "credentials.clientId");
            }

            if (!string.IsNullOrWhiteSpace(auth.ClientSecret))
            {
                content.Add(new StringContent(auth.ClientSecret), "credentials.clientSecret");
            }

            if (!string.IsNullOrWhiteSpace(auth.RefreshToken))
            {
                content.Add(new StringContent(auth.RefreshToken), "credentials.refreshToken");
            }

            if (!string.IsNullOrWhiteSpace(auth.UserEmail))
            {
                content.Add(new StringContent(auth.UserEmail), "credentials.userEmail");
            }
        }

        try
        {
            const string url = "uploads";
            var started = DateTimeOffset.UtcNow;

            using var response = await http.PostAsync(url, content, cancellationToken);
            var duration = DateTimeOffset.UtcNow - started;
            logger.LogInformation("HTTP POST {Url} -> {Status} in {Duration}ms", url, (int)response.StatusCode, (int)duration.TotalMilliseconds);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<BundleUploadResultModel>(stream, SerializerOptions, cancellationToken) ?? throw new InvalidOperationException("Empty upload response payload");

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            throw;
        }
    }
}