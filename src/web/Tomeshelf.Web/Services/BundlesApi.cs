using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Bundles;

namespace Tomeshelf.Web.Services;

/// <summary>
///     HTTP client for the Humble Bundle backend API.
/// </summary>
public sealed class BundlesApi : IBundlesApi
{
    public const string HttpClientName = "Web.Bundles";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly ILogger<BundlesApi> _logger;

    public BundlesApi(IHttpClientFactory httpClientFactory, ILogger<BundlesApi> logger)
    {
        _http = httpClientFactory.CreateClient(HttpClientName);
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BundleModel>> GetBundlesAsync(bool includeExpired, CancellationToken cancellationToken)
    {
        var url = $"bundles?includeExpired={includeExpired.ToString()
                                                          .ToLowerInvariant()}";
        var started = DateTimeOffset.UtcNow;

        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("HTTP GET {Url} -> {Status} in {Duration}ms", url, (int)response.StatusCode, (int)duration.TotalMilliseconds);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var bundles = await JsonSerializer.DeserializeAsync<List<BundleModel>>(stream, SerializerOptions, cancellationToken) ?? throw new InvalidOperationException("Empty bundle payload");

        return bundles;
    }
}
