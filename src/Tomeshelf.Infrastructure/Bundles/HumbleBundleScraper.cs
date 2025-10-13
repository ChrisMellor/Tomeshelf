using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tomeshelf.Infrastructure.Bundles;

/// <summary>
///     Scrapes the public Humble Bundle listing page to extract current bundles.
/// </summary>
public sealed class HumbleBundleScraper : IHumbleBundleScraper
{
    private static readonly Uri BundlesUri = new("https://www.humblebundle.com/bundles");
    private static readonly Uri SiteBaseUri = new("https://www.humblebundle.com/");
    private readonly HttpClient _httpClient;
    private readonly ILogger<HumbleBundleScraper> _logger;

    public HumbleBundleScraper(HttpClient httpClient, ILogger<HumbleBundleScraper> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ScrapedBundle>> ScrapeAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        _logger.LogInformation("Scraping Humble Bundle listings from {Uri}", BundlesUri);

        using var response =
            await _httpClient.GetAsync(BundlesUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var html = await reader.ReadToEndAsync(cancellationToken);

        var jsonPayload = ExtractEmbeddedJson(html);
        using var document = JsonDocument.Parse(jsonPayload);

        var now = DateTimeOffset.UtcNow;
        var bundles = new List<ScrapedBundle>();

        if (document.RootElement.TryGetProperty("data", out var dataElement))
            foreach (var categoryProperty in dataElement.EnumerateObject())
            {
                var category = categoryProperty.Name;
                if (categoryProperty.Value.TryGetProperty("mosaic", out var mosaics) &&
                    mosaics.ValueKind == JsonValueKind.Array)
                    foreach (var mosaic in mosaics.EnumerateArray())
                    {
                        if (!mosaic.TryGetProperty("products", out var products) ||
                            products.ValueKind != JsonValueKind.Array) continue;

                        foreach (var product in products.EnumerateArray())
                        {
                            if (!TryCreateBundle(product, category, now, out var bundle)) continue;

                            bundles.Add(bundle);
                        }
                    }
            }

        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("Scraped {Count} Humble Bundle listings in {Duration}ms", bundles.Count,
            (int)duration.TotalMilliseconds);

        return bundles;
    }

    private static bool TryCreateBundle(JsonElement product, string category, DateTimeOffset observedUtc,
        out ScrapedBundle bundle)
    {
        bundle = default!;

        if (!product.TryGetProperty("machine_name", out var machineElement) ||
            machineElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(machineElement.GetString()))
            return false;

        var machineName = machineElement.GetString()!;
        var stamp = GetString(product, "tile_stamp") ?? string.Empty;
        var title = GetString(product, "tile_name") ?? GetString(product, "marketing_blurb") ?? machineName;
        var shortName = GetString(product, "tile_short_name") ?? string.Empty;
        var shortDescription = GetString(product, "short_marketing_blurb") ?? string.Empty;

        var relativeUrl = GetString(product, "product_url") ?? string.Empty;
        var absoluteUrl = BuildAbsoluteUrl(relativeUrl);

        var tileImage = GetString(product, "tile_image") ?? string.Empty;
        var tileLogo = GetString(product, "tile_logo") ?? string.Empty;
        var heroImage = GetString(product, "high_res_tile_image") ?? tileImage;

        var startsAt = ParseDateTime(product, "start_date|datetime");
        var endsAt = ParseDateTime(product, "end_date|datetime");

        bundle = new ScrapedBundle(
            machineName,
            category,
            stamp,
            title,
            shortName,
            absoluteUrl,
            tileImage,
            tileLogo,
            heroImage ?? string.Empty,
            shortDescription,
            startsAt,
            endsAt,
            observedUtc);

        return true;
    }

    private static string BuildAbsoluteUrl(string relative)
    {
        if (string.IsNullOrWhiteSpace(relative)) return SiteBaseUri.ToString();

        if (Uri.TryCreate(relative, UriKind.Absolute, out var absolute)) return absolute.ToString();

        return new Uri(SiteBaseUri, relative.TrimStart('/')).ToString();
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
        {
            var value = property.GetString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return null;
    }

    private static DateTimeOffset? ParseDateTime(JsonElement element, string propertyName)
    {
        var text = GetString(element, propertyName);
        if (string.IsNullOrWhiteSpace(text)) return null;

        if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)) return parsed;

        return null;
    }

    private static string ExtractEmbeddedJson(string html)
    {
        const string marker = "{\"userOptions\":";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
            throw new InvalidOperationException("Unable to locate Humble Bundle JSON payload in the HTML response.");

        var span = html.AsSpan(start);
        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = 0; i < span.Length; i++)
        {
            var ch = span[i];

            if (escape)
            {
                escape = false;
                continue;
            }

            if (ch == '\\')
            {
                if (inString) escape = true;
                continue;
            }

            if (ch == '"')
            {
                inString = !inString;
                continue;
            }

            if (inString) continue;

            if (ch == '{')
            {
                depth++;
                continue;
            }

            if (ch == '}')
            {
                depth--;
                if (depth == 0) return span[..(i + 1)].ToString();
            }
        }

        throw new InvalidOperationException("Malformed Humble Bundle JSON payload.");
    }
}