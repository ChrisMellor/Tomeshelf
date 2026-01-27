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

public sealed class ShiftApi : IShiftApi
{
    public const string HttpClientName = "Web.Shift";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly HttpClient _http;
    private readonly ILogger<ShiftApi> _logger;

    public ShiftApi(IHttpClientFactory httpClientFactory, ILogger<ShiftApi> logger)
    {
        _http = httpClientFactory.CreateClient(HttpClientName);
        _logger = logger;
    }

    public async Task<RedeemResponseModel> RedeemCodeAsync(string code, CancellationToken cancellationToken)
    {
        const string url = "gearbox/redeem";
        var started = DateTimeOffset.UtcNow;

        using var response = await _http.PostAsJsonAsync(url, new RedeemRequestModel(code), SerializerOptions, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("HTTP POST {Url} -> {Status} in {Duration}ms", url, (int)response.StatusCode, (int)duration.TotalMilliseconds);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<RedeemResponseModel>(SerializerOptions, cancellationToken);

        return payload ?? throw new InvalidOperationException("Empty SHiFT payload");
    }
}
