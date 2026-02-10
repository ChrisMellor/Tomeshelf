using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tomeshelf.Executor.Jobs;

namespace Tomeshelf.Executor.Services;

public sealed class EndpointPingService : IEndpointPingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EndpointPingService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EndpointPingService" /> class.
    /// </summary>
    /// <param name="httpClientFactory">The http client factory.</param>
    /// <param name="logger">The logger.</param>
    public EndpointPingService(IHttpClientFactory httpClientFactory, ILogger<EndpointPingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    ///     Sends asynchronously.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="method">The method.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<EndpointPingResult> SendAsync(Uri target, string method, Dictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(target);

        using var request = BuildRequest(target, method, headers);
        var client = _httpClientFactory.CreateClient(TriggerEndpointJob.HttpClientName);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            var statusCode = (int)response.StatusCode;
            var message = response.ReasonPhrase ?? response.StatusCode.ToString();

            return new EndpointPingResult(response.IsSuccessStatusCode, statusCode, message, body, stopwatch.Elapsed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "Request to {Url} failed.", target);

            return new EndpointPingResult(false, null, ex.Message, null, stopwatch.Elapsed);
        }
    }

    /// <summary>
    ///     Adds the headers.
    /// </summary>
    /// <param name="headers">The headers.</param>
    /// <param name="request">The request.</param>
    private static void AddHeaders(Dictionary<string, string>? headers, HttpRequestMessage request)
    {
        if (headers is null)
        {
            return;
        }

        foreach (var header in headers)
        {
            if (request.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                continue;
            }

            request.Content ??= new StringContent(string.Empty);
            request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    /// <summary>
    ///     Builds the request.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="method">The method.</param>
    /// <param name="headers">The headers.</param>
    /// <returns>The result of the operation.</returns>
    private static HttpRequestMessage BuildRequest(Uri target, string? method, Dictionary<string, string>? headers)
    {
        var httpMethod = CreateMethod(method);
        var request = new HttpRequestMessage(httpMethod, target);
        AddHeaders(headers, request);

        return request;
    }

    /// <summary>
    ///     Creates the method.
    /// </summary>
    /// <param name="methodName">The method name.</param>
    /// <returns>The result of the operation.</returns>
    private static HttpMethod CreateMethod(string? methodName)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            return HttpMethod.Post;
        }

        try
        {
            return new HttpMethod(methodName);
        }
        catch (FormatException)
        {
            return HttpMethod.Post;
        }
    }
}