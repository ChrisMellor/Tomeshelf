using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Tomeshelf.Executor.Helpers;
using Tomeshelf.Executor.Models;

namespace Tomeshelf.Executor.Services;

/// <summary>
///     Executes configured endpoints on demand.
/// </summary>
public sealed class EndpointExecutor
{
    private const int MaximumBodyPreviewLength = 32_768;

    private readonly EndpointCatalog _catalog;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EndpointExecutor> _logger;

    public EndpointExecutor(EndpointCatalog catalog, IHttpClientFactory httpClientFactory, ILogger<EndpointExecutor> logger)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<EndpointExecutionResult> ExecuteAsync(string endpointId, EndpointExecutionRequest? request, CancellationToken cancellationToken)
    {
        if (!_catalog.TryGetDescriptor(endpointId, out var descriptor))
        {
            throw new KeyNotFoundException($"Unknown endpoint '{endpointId}'.");
        }

        return ExecuteAsync(descriptor, request, cancellationToken);
    }

    internal async Task<EndpointExecutionResult> ExecuteAsync(EndpointDescriptor descriptor, EndpointExecutionRequest? request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var (timeoutSource, effectiveTimeout) = CreateCancellationTokenSource(descriptor, request, cancellationToken);
        var effectiveCancellation = timeoutSource?.Token ?? cancellationToken;

        var httpClient = _httpClientFactory.CreateClient("executor");

        using var message = BuildRequest(descriptor, request, out var targetUri);

        var started = Stopwatch.GetTimestamp();

        HttpResponseMessage? response = null;
        var responseBody = string.Empty;

        try
        {
            response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseContentRead, effectiveCancellation)
                                       .ConfigureAwait(false);
            responseBody = await response.Content.ReadAsStringAsync(effectiveCancellation)
                                         .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutSource is not null && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Execution of endpoint {EndpointId} timed out after {Timeout}.", descriptor.Id, effectiveTimeout);

            throw;
        }
        finally
        {
            timeoutSource?.Dispose();
        }

        var elapsed = Stopwatch.GetElapsedTime(started);
        var statusCode = (int)(response?.StatusCode ?? HttpStatusCode.InternalServerError);
        var reason = response?.ReasonPhrase ?? string.Empty;
        var headers = ProjectHeaders(response);

        string? contentType = null;
        if (response?.Content is not null)
        {
            contentType = response.Content.Headers.ContentType?.ToString();

            if (string.IsNullOrWhiteSpace(contentType) && response.Content.Headers.TryGetValues("Content-Type", out var headerValues))
            {
                contentType = headerValues.FirstOrDefault();
            }
        }

        var finalBody = Truncate(responseBody, MaximumBodyPreviewLength, out var truncated);

        return new EndpointExecutionResult
        {
                EndpointId = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                Method = message.Method.Method,
                Target = targetUri,
                Success = response is not null && response.IsSuccessStatusCode,
                StatusCode = statusCode,
                ReasonPhrase = reason,
                ContentType = contentType,
                Body = finalBody,
                BodyTruncated = truncated,
                Headers = headers,
                DurationMilliseconds = elapsed.TotalMilliseconds,
                ExecutedAt = DateTimeOffset.UtcNow
        };
    }

    private static (CancellationTokenSource? Source, TimeSpan? Timeout) CreateCancellationTokenSource(EndpointDescriptor descriptor, EndpointExecutionRequest? request, CancellationToken cancellationToken)
    {
        var overrideTimeout = request?.TimeoutSeconds;
        TimeSpan? timeout = null;

        if (overrideTimeout.HasValue)
        {
            if (overrideTimeout.Value <= 0)
            {
                throw new InvalidOperationException("TimeoutSeconds must be greater than zero when supplied.");
            }

            timeout = TimeSpan.FromSeconds(overrideTimeout.Value);
        }
        else
        {
            timeout = descriptor.Timeout;
        }

        if (!timeout.HasValue)
        {
            return (null, null);
        }

        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedSource.CancelAfter(timeout.Value);

        return (linkedSource, timeout);
    }

    private static IDictionary<string, string[]> ProjectHeaders(HttpResponseMessage? response)
    {
        if (response is null)
        {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }

        var headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in response.Headers)
        {
            headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in response.Content.Headers)
        {
            headers[header.Key] = header.Value.ToArray();
        }

        return headers;
    }

    private static HttpRequestMessage BuildRequest(EndpointDescriptor descriptor, EndpointExecutionRequest? request, out string targetUri)
    {
        var methodString = descriptor.Method;
        if (request is not null && !string.IsNullOrWhiteSpace(request.Method))
        {
            methodString = HttpMethodUtilities.Normalise(request.Method);
        }

        var httpMethod = HttpMethodUtilities.Resolve(methodString);

        var baseUri = ResolveRequestUri(descriptor, request);
        var uriWithQuery = ApplyQuery(baseUri, request?.Query);

        var message = new HttpRequestMessage(httpMethod, uriWithQuery);

        BuildRequestBody(message, descriptor, request);

        ApplyHeaders(message, descriptor.Headers);
        if (request?.Headers is not null)
        {
            ApplyHeaders(message, request.Headers);
        }

        targetUri = uriWithQuery.ToString();

        return message;
    }

    private static Uri ResolveRequestUri(EndpointDescriptor descriptor, EndpointExecutionRequest? request)
    {
        if (!string.IsNullOrWhiteSpace(request?.Url))
        {
            if (!Uri.TryCreate(request.Url.Trim(), UriKind.Absolute, out var overrideUri))
            {
                throw new InvalidOperationException($"The override URL '{request.Url}' is not a valid absolute URI.");
            }

            return overrideUri;
        }

        return descriptor.Uri;
    }

    private static Uri ApplyQuery(Uri baseUri, IDictionary<string, string>? query)
    {
        if (query is null || (query.Count == 0))
        {
            return baseUri;
        }

        var dictionary = query.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
                              .ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value, StringComparer.OrdinalIgnoreCase);

        var uri = new Uri(QueryHelpers.AddQueryString(baseUri.ToString(), dictionary));

        return uri;
    }

    private static void BuildRequestBody(HttpRequestMessage message, EndpointDescriptor descriptor, EndpointExecutionRequest? request)
    {
        var payload = request?.Body ?? descriptor.BodyTemplate;

        if (string.IsNullOrEmpty(payload))
        {
            return;
        }

        if (!descriptor.AllowBody)
        {
            throw new InvalidOperationException($"Endpoint '{descriptor.DisplayName}' does not accept a request body.");
        }

        var contentType = descriptor.BodyContentType ?? "application/json";
        var content = new StringContent(payload, Encoding.UTF8, contentType);
        message.Content = content;
    }

    private static void ApplyHeaders(HttpRequestMessage message, IEnumerable<KeyValuePair<string, string>> headers)
    {
        foreach (var header in headers)
        {
            if (string.IsNullOrWhiteSpace(header.Key))
            {
                continue;
            }

            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                if (message.Content is not null)
                {
                    message.Content.Headers.Remove("Content-Type");
                    message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                continue;
            }

            message.Headers.Remove(header.Key);
            message.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    private static string Truncate(string value, int maxLength, out bool truncated)
    {
        if (value.Length <= maxLength)
        {
            truncated = false;

            return value;
        }

        truncated = true;

        return value[..maxLength] + "...";
    }
}