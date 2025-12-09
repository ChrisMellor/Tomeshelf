using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor.Services;

/// <summary>
///     Default implementation that reads Aspire-provided configuration and queries discovery endpoints.
/// </summary>
public sealed class ApiEndpointDiscoveryService : IApiEndpointDiscoveryService
{
    public const string HttpClientName = "ExecutorDiscovery";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiEndpointDiscoveryService> _logger;

    public ApiEndpointDiscoveryService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<ApiEndpointDiscoveryService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public Task<IReadOnlyList<ApiServiceDescriptor>> GetApisAsync(CancellationToken cancellationToken)
    {
        var servicesSection = _configuration.GetSection("services");
        if (!servicesSection.Exists())
        {
            return Task.FromResult<IReadOnlyList<ApiServiceDescriptor>>(Array.Empty<ApiServiceDescriptor>());
        }

        var services = new List<ApiServiceDescriptor>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var serviceSection in servicesSection.GetChildren())
        {
            var serviceName = serviceSection.Key;
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                continue;
            }

            foreach (var transportSection in serviceSection.GetChildren())
            {
                var transportName = transportSection.Key;
                if (string.IsNullOrWhiteSpace(transportName))
                {
                    continue;
                }

                foreach (var endpointSection in transportSection.GetChildren())
                {
                    var rawAddress = endpointSection.Value?.Trim();
                    if (string.IsNullOrWhiteSpace(rawAddress) || !Uri.TryCreate(rawAddress, UriKind.Absolute, out var parsed))
                    {
                        continue;
                    }

                    var normalized = parsed.ToString()
                                           .TrimEnd('/');
                    if (!seen.Add(normalized))
                    {
                        continue;
                    }

                    var displayName = $"{serviceName} ({transportName.ToUpperInvariant()})";
                    services.Add(new ApiServiceDescriptor(serviceName, displayName, normalized));
                }
            }
        }

        var ordered = services.OrderBy(s => s.ServiceName, StringComparer.OrdinalIgnoreCase)
                              .ThenBy(s => s.BaseAddress, StringComparer.OrdinalIgnoreCase)
                              .ToList();

        return Task.FromResult<IReadOnlyList<ApiServiceDescriptor>>(ordered);
    }

    public async Task<IReadOnlyList<ExecutorDiscoveredEndpoint>> GetEndpointsAsync(string baseAddress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(baseAddress) || !Uri.TryCreate(baseAddress, UriKind.Absolute, out var baseUri))
        {
            return Array.Empty<ExecutorDiscoveredEndpoint>();
        }

        var discoveryUri = new Uri(baseUri, ExecutorDiscoveryConstants.DefaultPath);

        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.GetAsync(discoveryUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Discovery request to {Uri} failed with status code {StatusCode}.", discoveryUri, response.StatusCode);

                return Array.Empty<ExecutorDiscoveredEndpoint>();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var document = await JsonSerializer.DeserializeAsync<ExecutorDiscoveryDocument>(stream, SerializerOptions, cancellationToken);

            return document?.Endpoints?.ToList() ?? new List<ExecutorDiscoveredEndpoint>();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            _logger.LogWarning(ex, "Failed to read executor discovery document from {Uri}.", discoveryUri);

            return Array.Empty<ExecutorDiscoveredEndpoint>();
        }
    }
}