using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Models.Shift;

namespace Tomeshelf.Web.Services;

public sealed class ShiftApi : IShiftApi
{
    public const string HttpClientName = "Web.Shift";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private readonly HttpClient _http;
    private readonly ILogger<ShiftApi> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ShiftApi" /> class.
    /// </summary>
    /// <param name="httpClientFactory">The http client factory.</param>
    /// <param name="logger">The logger.</param>
    public ShiftApi(IHttpClientFactory httpClientFactory, ILogger<ShiftApi> logger)
    {
        _http = httpClientFactory.CreateClient(HttpClientName);
        _logger = logger;
    }

    /// <summary>
    ///     Redeems the code asynchronously.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
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

    /// <summary>
    ///     Gets the accounts asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<IReadOnlyList<ShiftAccountModel>> GetAccountsAsync(CancellationToken cancellationToken)
    {
        const string url = "config/shift";
        var started = DateTimeOffset.UtcNow;

        using var response = await _http.GetAsync(url, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("HTTP GET {Url} -> {Status} in {Duration}ms", url, (int)response.StatusCode, (int)duration.TotalMilliseconds);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<List<ShiftAccountModel>>(SerializerOptions, cancellationToken);

        return payload ?? [];
    }

    /// <summary>
    ///     Creates the account asynchronously.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<bool> CreateAccountAsync(ShiftAccountEditorModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        const string url = "config/shift";
        var started = DateTimeOffset.UtcNow;

        using var response = await _http.PostAsJsonAsync(url, model, SerializerOptions, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("HTTP POST {Url} -> {Status} in {Duration}ms", url, (int)response.StatusCode, (int)duration.TotalMilliseconds);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();

        return true;
    }

    /// <summary>
    ///     Deletes the account asynchronously.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteAccountAsync(int id, CancellationToken cancellationToken)
    {
        var url = $"config/shift/{id}";
        var started = DateTimeOffset.UtcNow;

        using var response = await _http.DeleteAsync(url, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("HTTP DELETE {Url} -> {Status} in {Duration}ms", url, (int)response.StatusCode, (int)duration.TotalMilliseconds);

        response.EnsureSuccessStatusCode();
    }
}
