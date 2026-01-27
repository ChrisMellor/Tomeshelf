using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Paissa.Application.Abstractions.External;
using Tomeshelf.Paissa.Domain.Entities;

namespace Tomeshelf.Paissa.Infrastructure.Services.External;

/// <summary>
///     Provides methods for interacting with the Paissa API to retrieve world and housing data.
/// </summary>
/// <remarks>
///     This client requires an instance of HttpClient for making HTTP requests and an ILogger for logging.
///     It is designed to handle API calls to retrieve information about worlds, including error handling for scenarios
///     where a world is not found.
/// </remarks>
public sealed class PaissaClient : IPaissaClient
{
    public const string HttpClientName = "Paissa.Api";
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaissaClient> _logger;

    /// <summary>
    ///     Initializes a new instance of the PaissaClient class using the specified HTTP client and logger.
    /// </summary>
    /// <param name="httpClientFactory">Factory used to resolve the named HTTP client for PaissaDB requests.</param>
    /// <param name="logger">
    ///     The logger used to record informational messages, warnings, and errors related to PaissaClient
    ///     operations.
    /// </param>
    public PaissaClient(IHttpClientFactory httpClientFactory, ILogger<PaissaClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
        _logger = logger;
    }

    /// <summary>
    ///     Asynchronously retrieves the details of a specified world from the Paissa database.
    /// </summary>
    /// <remarks>A warning is logged if the specified world is not found in the Paissa database.</remarks>
    /// <param name="worldId">The unique identifier of the world to retrieve. Must be a positive integer.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the details of the requested world
    ///     as a PaissaWorld object.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the Paissa database returns an empty payload for the specified
    ///     world.
    /// </exception>
    public async Task<PaissaWorld> GetWorldAsync(int worldId, CancellationToken cancellationToken)
    {
        var uri = $"worlds/{worldId}";
        try
        {
            var result = await _httpClient.GetFromJsonAsync<PaissaWorldDto>(uri, cancellationToken);
            if (result is null)
            {
                throw new InvalidOperationException("PaissaDB returned an empty payload.");
            }

            return PaissaDtoMapper.MapWorld(result);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            _logger.LogWarning(ex, "World {WorldId} not found in PaissaDB.", worldId);

            throw;
        }
    }

}
