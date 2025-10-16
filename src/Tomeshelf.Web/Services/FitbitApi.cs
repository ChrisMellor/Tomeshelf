#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Services;

/// <summary>
///     HTTP client wrapper for the Fitbit backend API.
/// </summary>
public sealed class FitbitApi(HttpClient httpClient, ILogger<FitbitApi> logger) : IFitbitApi
{
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    /// <inheritdoc />
    public async Task<FitbitDashboardModel?> GetDashboardAsync(string? date, bool refresh, string returnUrl, CancellationToken cancellationToken)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(date))
        {
            parameters.Add($"date={Uri.EscapeDataString(date)}");
        }

        if (refresh)
        {
            parameters.Add("refresh=true");
        }

        parameters.Add($"returnUrl={Uri.EscapeDataString(returnUrl)}");

        var query = $"?{string.Join("&", parameters)}";

        var url = $"api/Fitbit/Dashboard{query}";
        var started = DateTimeOffset.UtcNow;
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        logger.LogInformation("HTTP GET {Url} -> {Status} in {Duration}ms", url, (int)response.StatusCode, (int)duration.TotalMilliseconds);

        if (IsRedirectStatus(response.StatusCode))
        {
            var location = response.Headers.Location;
            if (location is null)
            {
                throw new InvalidOperationException("Fitbit API returned a redirect without a location header.");
            }

            if (!location.IsAbsoluteUri && httpClient.BaseAddress is not null)
            {
                location = new Uri(httpClient.BaseAddress, location);
            }

            throw new FitbitAuthorizationRequiredException(location);
        }

        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new FitbitBackendUnavailableException(message);
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new FitbitBackendUnavailableException(string.IsNullOrWhiteSpace(message)
                                                                ? "Fitbit rate limit reached. Please wait a moment and try again."
                                                                : message);
        }

        if (response.StatusCode == HttpStatusCode.BadGateway)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new FitbitBackendUnavailableException(message);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var location = response.Headers.Location ?? new Uri("/fitness", UriKind.Relative);
            if (!location.IsAbsoluteUri && httpClient.BaseAddress is not null)
            {
                location = new Uri(httpClient.BaseAddress, location);
            }

            throw new FitbitAuthorizationRequiredException(location);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var model = await JsonSerializer.DeserializeAsync<FitbitDashboardModel>(stream, SerializerOptions, cancellationToken);

        if (model is null)
        {
            throw new InvalidOperationException("Received an empty Fitbit dashboard payload.");
        }

        return model;
    }

    /// <inheritdoc />
    public async Task<Uri> ResolveAuthorizationAsync(Uri authorizeEndpoint, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorizeEndpoint);

        using var request = new HttpRequestMessage(HttpMethod.Get, authorizeEndpoint);
        var started = DateTimeOffset.UtcNow;
        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        logger.LogInformation("HTTP GET {Url} -> {Status} in {Duration}ms (authorization resolve)", authorizeEndpoint, (int)response.StatusCode, (int)duration.TotalMilliseconds);

        if (!IsRedirectStatus(response.StatusCode))
        {
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Fitbit authorization endpoint returned unexpected status {(int)response.StatusCode}. Response: {payload}");
        }

        var location = response.Headers.Location;
        if (location is null)
        {
            throw new InvalidOperationException("Fitbit authorization endpoint returned a redirect without a location header.");
        }

        if (!location.IsAbsoluteUri && httpClient.BaseAddress is not null)
        {
            location = new Uri(httpClient.BaseAddress, location);
        }

        return location;
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        return new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    private static bool IsRedirectStatus(HttpStatusCode statusCode)
    {
        return (statusCode == HttpStatusCode.Redirect) || (statusCode == HttpStatusCode.RedirectKeepVerb) || (statusCode == HttpStatusCode.RedirectMethod) || (statusCode == HttpStatusCode.MovedPermanently) || (statusCode == HttpStatusCode.SeeOther) || (statusCode == HttpStatusCode.TemporaryRedirect) || (statusCode == HttpStatusCode.PermanentRedirect);
    }
}

public sealed class FitbitAuthorizationRequiredException : Exception
{
    public FitbitAuthorizationRequiredException(Uri location) : base("Fitbit authorization is required.")
    {
        Location = location;
    }

    public Uri Location { get; }
}

public sealed class FitbitBackendUnavailableException : Exception
{
    public FitbitBackendUnavailableException(string? message) : base(BuildMessage(message)) { }

    private static string BuildMessage(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "Fitbit service is unavailable. Please try again in a moment.";
        }

        var trimmed = raw.Trim();
        if (trimmed.StartsWith("<", StringComparison.Ordinal))
        {
            return "Fitbit service is unavailable. Please try again in a moment.";
        }

        if (trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            try
            {
                using var document = JsonDocument.Parse(trimmed);
                if ((document.RootElement.ValueKind == JsonValueKind.Object) && document.RootElement.TryGetProperty("message", out var messageElement) && (messageElement.ValueKind == JsonValueKind.String))
                {
                    var parsedMessage = messageElement.GetString();
                    if (!string.IsNullOrWhiteSpace(parsedMessage))
                    {
                        return parsedMessage.Trim();
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore invalid JSON and fall back to the original payload.
            }
        }

        if (trimmed.StartsWith("\"", StringComparison.Ordinal) && trimmed.EndsWith("\"", StringComparison.Ordinal))
        {
            try
            {
                trimmed = JsonSerializer.Deserialize<string>(trimmed) ?? trimmed;
            }
            catch (JsonException)
            {
                // Ignore invalid JSON string and fall back to the original payload.
            }
        }

        return trimmed;
    }
}
