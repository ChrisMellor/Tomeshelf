using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tomeshelf.Paissa.Api.Models;

namespace Tomeshelf.Paissa.Api.Services;

internal sealed class PaissaClient : IPaissaClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaissaClient> _logger;

    public PaissaClient(HttpClient httpClient, ILogger<PaissaClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaissaWorldDto> GetWorldAsync(int worldId, CancellationToken cancellationToken)
    {
        var uri = $"worlds/{worldId}";
        try
        {
            var result = await _httpClient.GetFromJsonAsync<PaissaWorldDto>(uri, cancellationToken);
            if (result is null)
            {
                throw new InvalidOperationException("PaissaDB returned an empty payload.");
            }

            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            _logger.LogWarning(ex, "World {WorldId} not found in PaissaDB.", worldId);

            throw;
        }
    }
}