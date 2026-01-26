using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Shift;

namespace Tomeshelf.Web.Services;

public sealed class ShiftApi(HttpClient http, ILogger<ShiftApi> logger) : IShiftApi
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<RedeemResponseModel> RedeemCodeAsync(string code, CancellationToken cancellationToken)
    {
        const string url = "gearbox/redeem";
        var started = DateTimeOffset.UtcNow;

        using var response = await http.PostAsJsonAsync(url, new RedeemRequestModel(code), SerializerOptions, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        logger.LogInformation("HTTP POST {Url} -> {Status} in {Duration}ms", url, (int)response.StatusCode, (int)duration.TotalMilliseconds);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<RedeemResponseModel>(SerializerOptions, cancellationToken);

        return payload ?? throw new InvalidOperationException("Empty SHiFT payload");
    }
}
