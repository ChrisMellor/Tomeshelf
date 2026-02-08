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
public sealed class FitbitApi : IFitbitApi
{
    public const string HttpClientName = "Web.Fitbit";
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();
    private readonly HttpClient _httpClient;
    private readonly ILogger<FitbitApi> _logger;

    public FitbitApi(IHttpClientFactory httpClientFactory, ILogger<FitbitApi> logger)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<FitbitDashboardModel> GetDashboardAsync(string date, bool refresh, string returnUrl, CancellationToken cancellationToken)
    {
        var url = BuildQuery("api/Fitbit/Dashboard", date, refresh, returnUrl);

        return await GetPayloadAsync<FitbitDashboardModel>(url, "Fitbit dashboard", cancellationToken)
           .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<FitbitOverviewModel> GetOverviewAsync(string date, bool refresh, string returnUrl, CancellationToken cancellationToken)
    {
        var url = BuildQuery("api/Fitbit/Overview", date, refresh, returnUrl);

        return await GetPayloadAsync<FitbitOverviewModel>(url, "Fitbit overview", cancellationToken)
           .ConfigureAwait(false);
    }

    private static string BuildQuery(string basePath, string date, bool refresh, string returnUrl)
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

        return $"{basePath}{query}";
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        return new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    private async Task<T> GetPayloadAsync<T>(string url, string payloadName, CancellationToken cancellationToken)
    {
        var started = DateTimeOffset.UtcNow;
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("HTTP GET {Url} -> {Status} in {Duration}ms", url, (int)response.StatusCode, (int)duration.TotalMilliseconds);

        if (IsRedirectStatus(response.StatusCode))
        {
            var location = response.Headers.Location;
            if (location is null)
            {
                throw new InvalidOperationException("Fitbit API returned a redirect without a location header.");
            }

            if (!location.IsAbsoluteUri && _httpClient.BaseAddress is not null)
            {
                location = new Uri(_httpClient.BaseAddress, location);
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
            if (!location.IsAbsoluteUri && _httpClient.BaseAddress is not null)
            {
                location = new Uri(_httpClient.BaseAddress, location);
            }

            throw new FitbitAuthorizationRequiredException(location);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var model = await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions, cancellationToken);

        if (model is null)
        {
            throw new InvalidOperationException($"Received an empty {payloadName} payload.");
        }

        return model;
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
    public FitbitBackendUnavailableException(string message) : base(BuildMessage(message)) { }

    private static string BuildMessage(string raw)
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
            catch (JsonException) { }
        }

        if (trimmed.StartsWith("\"", StringComparison.Ordinal) && trimmed.EndsWith("\"", StringComparison.Ordinal))
        {
            try
            {
                trimmed = JsonSerializer.Deserialize<string>(trimmed) ?? trimmed;
            }
            catch (JsonException) { }
        }

        return trimmed;
    }
}