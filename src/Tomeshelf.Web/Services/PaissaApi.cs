using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Models.Paissa;

namespace Tomeshelf.Web.Services;

public sealed class PaissaApi(HttpClient http, ILogger<PaissaApi> logger) : IPaissaApi
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public async Task<PaissaWorldModel> GetWorldAsync(CancellationToken cancellationToken)
    {
        const string url = "paissa/world";
        var started = DateTimeOffset.UtcNow;

        using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        logger.LogInformation("HTTP GET {Url} -> {Status} in {Duration}ms", url, (int)response.StatusCode, (int)duration.TotalMilliseconds);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var world = await JsonSerializer.DeserializeAsync<PaissaWorldModel>(stream, SerializerOptions, cancellationToken) ?? throw new InvalidOperationException("Empty Paissa payload");

        return world;
    }
}