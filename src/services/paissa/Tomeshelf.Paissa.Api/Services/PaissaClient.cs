using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Paissa.Api.Models;
using Tomeshelf.Paissa.Application;

namespace Tomeshelf.Paissa.Api.Services;

internal sealed class PaissaClient(HttpClient httpClient, ILogger<PaissaClient> logger) : IPaissaClient
{
    public async Task<PaissaWorldDto> GetWorldAsync(int worldId, CancellationToken cancellationToken)
    {
        var uri = $"worlds/{worldId}";
        try
        {
            var result = await httpClient.GetFromJsonAsync<PaissaWorldDto>(uri, cancellationToken);
            if (result is null)
            {
                throw new InvalidOperationException("PaissaDB returned an empty payload.");
            }

            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            logger.LogWarning(ex, "World {WorldId} not found in PaissaDB.", worldId);

            throw;
        }
    }
}