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

    /// <summary>
    ///     Initializes a new instance of the <see cref="FitbitApi" /> class.
    /// </summary>
    /// <param name="httpClientFactory">The http client factory.</param>
    /// <param name="logger">The logger.</param>
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

    /// <summary>
    ///     Builds the query.
    /// </summary>
    /// <param name="basePath">The base path.</param>
    /// <param name="date">The date.</param>
    /// <param name="refresh">The refresh.</param>
    /// <param name="returnUrl">The return url.</param>
    /// <returns>The resulting string.</returns>
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

    /// <summary>
    ///     Creates the serializer options.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    private static JsonSerializerOptions CreateSerializerOptions()
    {
        return new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    /// <summary>
    ///     Gets the payload asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of t.</typeparam>
    /// <param name="url">The url.</param>
    /// <param name="payloadName">The payload name.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
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

    /// <summary>
    ///     Determines whether the specified status code is a redirect status.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    private static bool IsRedirectStatus(HttpStatusCode statusCode)
    {
        return (statusCode == HttpStatusCode.Redirect) || (statusCode == HttpStatusCode.RedirectKeepVerb) || (statusCode == HttpStatusCode.RedirectMethod) || (statusCode == HttpStatusCode.MovedPermanently) || (statusCode == HttpStatusCode.SeeOther) || (statusCode == HttpStatusCode.TemporaryRedirect) || (statusCode == HttpStatusCode.PermanentRedirect);
    }
}