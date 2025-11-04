using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Tomeshelf.Executor.Models;
using Tomeshelf.Executor.Options;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor.Services;

/// <summary>
///     Background service that hydrates endpoints via downstream discovery endpoints.
/// </summary>
public sealed class EndpointAutoDiscoveryService : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static readonly TimeSpan DefaultRefreshInterval = TimeSpan.FromMinutes(15);
    private static readonly Regex IdentifierSanitizer = new("[^a-zA-Z0-9:_\\-]", RegexOptions.Compiled);

    private readonly EndpointCatalog _catalog;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly bool _isDocker;
    private readonly ILogger<EndpointAutoDiscoveryService> _logger;
    private readonly ExecutorOptions _options;

    public EndpointAutoDiscoveryService(EndpointCatalog catalog, IHttpClientFactory httpClientFactory, IOptions<ExecutorOptions> options, ILogger<EndpointAutoDiscoveryService> logger)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _isDocker = RuntimeEnvironment.IsRunningInDocker();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.AutoDiscovery.Count == 0)
        {
            _logger.LogInformation("Executor auto-discovery disabled (no sources configured).");

            return;
        }

        var refreshInterval = _options.AutoDiscoveryRefreshIntervalSeconds <= 0
                ? (TimeSpan?)null
                : TimeSpan.FromSeconds(_options.AutoDiscoveryRefreshIntervalSeconds);

        do
        {
            await RefreshAllAsync(stoppingToken)
                   .ConfigureAwait(false);

            if (refreshInterval is null)
            {
                break;
            }

            try
            {
                await Task.Delay(refreshInterval.Value, stoppingToken)
                          .ConfigureAwait(false);
            }
            catch (TaskCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
        while (!stoppingToken.IsCancellationRequested);
    }

    private async Task RefreshAllAsync(CancellationToken cancellationToken)
    {
        foreach (var source in _options.AutoDiscovery)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await RefreshSourceAsync(source, cancellationToken)
                   .ConfigureAwait(false);
        }
    }

    private async Task RefreshSourceAsync(AutoDiscoverySource source, CancellationToken cancellationToken)
    {
        if (source is null)
        {
            return;
        }

        var requestUri = BuildDiscoveryUri(source);
        if (requestUri is null)
        {
            _logger.LogWarning("Unable to build discovery URI for source {Source}.", source.Service);

            return;
        }

        var httpClient = _httpClientFactory.CreateClient("executor");

        try
        {
            using var response = await httpClient.GetAsync(requestUri, cancellationToken)
                                                 .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Discovery request for {Source} returned status code {StatusCode}.", source.Service, response.StatusCode);

                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                                                   .ConfigureAwait(false);
            var document = await JsonSerializer.DeserializeAsync<ExecutorDiscoveryDocument>(stream, JsonOptions, cancellationToken)
                                               .ConfigureAwait(false);
            if (document?.Endpoints is null)
            {
                _logger.LogWarning("Discovery response for {Source} did not contain endpoints.", source.Service);
                _catalog.ReplaceDiscoveredEndpoints(source.Service, Array.Empty<EndpointDefinition>());

                return;
            }

            var definitions = new List<EndpointDefinition>();

            foreach (var endpoint in document.Endpoints)
            {
                var definition = CreateDefinition(source, endpoint);
                if (definition is not null)
                {
                    definitions.Add(definition);
                }
            }

            _catalog.ReplaceDiscoveredEndpoints(source.Service, definitions);
            _logger.LogInformation("Discovered {Count} endpoints for {Source}.", definitions.Count, source.Service);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Shutdown requested; ignore.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh discovery for source {Source}.", source.Service);
        }
    }

    private Uri? BuildDiscoveryUri(AutoDiscoverySource source)
    {
        var discoveryPath = NormalizePath(source.DiscoveryPath);

        if (!string.IsNullOrWhiteSpace(source.BaseUrl))
        {
            if (!Uri.TryCreate(source.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                _logger.LogWarning("BaseUrl '{BaseUrl}' for {Source} is not a valid absolute URI.", source.BaseUrl, source.Service);

                return null;
            }

            return new Uri(baseUri, discoveryPath);
        }

        if (string.IsNullOrWhiteSpace(source.Service))
        {
            return null;
        }

        var scheme = !string.IsNullOrWhiteSpace(source.Scheme)
                ? source.Scheme.Trim()
                : ResolveDefaultScheme();

        var builder = new StringBuilder();
        builder.Append(scheme);
        builder.Append("://");
        builder.Append(source.Service.Trim());

        if (source.Port.HasValue)
        {
            builder.Append(':');
            builder.Append(source.Port.Value);
        }

        if (!Uri.TryCreate(builder.ToString(), UriKind.Absolute, out var baseServiceUri))
        {
            _logger.LogWarning("Unable to construct base URI for {Source} using scheme {Scheme}.", source.Service, scheme);

            return null;
        }

        return new Uri(baseServiceUri, discoveryPath);
    }

    private EndpointDefinition? CreateDefinition(AutoDiscoverySource source, ExecutorDiscoveredEndpoint endpoint)
    {
        if (endpoint is null || string.IsNullOrWhiteSpace(endpoint.Method) || string.IsNullOrWhiteSpace(endpoint.RelativePath))
        {
            return null;
        }

        var idSeed = !string.IsNullOrWhiteSpace(endpoint.Id)
                ? endpoint.Id
                : $"{endpoint.Method}:{endpoint.RelativePath}";

        var sanitizedId = IdentifierSanitizer.Replace(idSeed, "-")
                                             .Trim('-');
        if (string.IsNullOrWhiteSpace(sanitizedId))
        {
            sanitizedId = Guid.NewGuid()
                              .ToString("N");
        }

        var id = $"auto:{source.Service}:{sanitizedId}";

        var displayName = endpoint.DisplayName;
        if (!string.IsNullOrWhiteSpace(source.DisplayPrefix))
        {
            displayName = $"{source.DisplayPrefix}{displayName ?? endpoint.RelativePath}";
        }

        var definition = new EndpointDefinition
        {
                Id = id,
                DisplayName = displayName ?? id,
                Description = endpoint.Description,
                Method = endpoint.Method,
                Path = NormalizePath(endpoint.RelativePath),
                BodyTemplate = null,
                BodyContentType = null,
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                AllowBody = endpoint.AllowBody,
                Schedule = source.Schedule,
                TimeZone = source.TimeZone,
                TimeoutSeconds = null
        };

        if (!string.IsNullOrWhiteSpace(source.BaseUrl))
        {
            if (Uri.TryCreate(source.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                definition.Url = new Uri(baseUri, definition.Path).ToString();
                definition.Path = null;
            }
        }
        else
        {
            definition.Service = source.Service;
            if (source.Port.HasValue)
            {
                definition.Port = source.Port;
            }

            if (!string.IsNullOrWhiteSpace(source.Scheme) && string.Equals(source.Scheme, "https", StringComparison.OrdinalIgnoreCase) && (definition.Port == null))
            {
                // Force use of https via explicit URL to avoid default http resolution.
                definition.Url = $"{source.Scheme.Trim()
                                          .ToLowerInvariant()}://{source.Service.Trim()}{NormalizePath(endpoint.RelativePath)}";
                definition.Path = null;
                definition.Service = null;
            }
        }

        return definition;
    }

    private string ResolveDefaultScheme()
    {
        if (_isDocker)
        {
            return string.IsNullOrWhiteSpace(_options.DefaultDockerScheme)
                    ? "http"
                    : _options.DefaultDockerScheme.Trim();
        }

        return string.IsNullOrWhiteSpace(_options.DefaultScheme)
                ? "https"
                : _options.DefaultScheme.Trim();
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return ExecutorDiscoveryConstants.DefaultPath;
        }

        return path.StartsWith("/", StringComparison.Ordinal)
                ? path
                : "/" + path;
    }
}