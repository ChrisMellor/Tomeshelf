using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Models.Paissa;

namespace Tomeshelf.Web.Services;

public sealed class PaissaApi : IPaissaApi
{
    public const string HttpClientName = "Web.Paissa";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly ILogger<PaissaApi> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PaissaApi" /> class.
    /// </summary>
    /// <param name="httpClientFactory">The http client factory.</param>
    /// <param name="logger">The logger.</param>
    public PaissaApi(IHttpClientFactory httpClientFactory, ILogger<PaissaApi> logger)
    {
        _http = httpClientFactory.CreateClient(HttpClientName);
        _logger = logger;
    }

    /// <summary>
    ///     Gets the world asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<PaissaWorldModel> GetWorldAsync(CancellationToken cancellationToken)
    {
        const string url = "paissa/world";
        var started = DateTimeOffset.UtcNow;

        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("HTTP GET {Url} -> {Status} in {Duration}ms", url, (int)response.StatusCode, (int)duration.TotalMilliseconds);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var world = await JsonSerializer.DeserializeAsync<PaissaWorldModel>(stream, SerializerOptions, cancellationToken) ?? throw new InvalidOperationException("Empty Paissa payload");

        return world;
    }
}
