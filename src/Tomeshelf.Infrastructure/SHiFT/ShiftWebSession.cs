using AngleSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.SHiFT;

namespace Tomeshelf.Infrastructure.SHiFT;

public sealed class ShiftWebSession : IAsyncDisposable, IShiftWebSession
{
    private readonly CookieContainer _cookies = new CookieContainer();
    private readonly IBrowsingContext _dom;
    private readonly HttpClient _http;

    public ShiftWebSession()
    {
        var handler = new SocketsHttpHandler
        {
            CookieContainer = _cookies,
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        };

        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://shift.gearboxsoftware.com/")
        };

        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf.SHiFT.Api/1.0");
        _http.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/json");

        var config = Configuration.Default;
        _dom = BrowsingContext.New(config);
    }

    public ValueTask DisposeAsync()
    {
        _http.Dispose();

        return ValueTask.CompletedTask;
    }

    public async Task<string> BuildRedeemBodyAsync(string code, string csrfToken, string service, CancellationToken ct = default)
    {
        var url = $"entitlement_offer_codes?code={Uri.EscapeDataString(code)}";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("X-Requested-With", "XMLHttpRequest");
        req.Headers.Add("X-CSRF-Token", csrfToken);
        req.Headers.Accept.Clear();
        req.Headers.Accept.ParseAdd("text/html");

        using var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();

        var html = await res.Content.ReadAsStringAsync(ct);

        var doc = await _dom.OpenAsync(r => r.Content(html), ct);

        var targetForm = doc.QuerySelectorAll("form")
                            .FirstOrDefault(f => f.QuerySelectorAll("input")
                                                  .Any(i => string.Equals(i.GetAttribute("value"), service, StringComparison.OrdinalIgnoreCase)));

        if (targetForm is null)
        {
            throw new InvalidOperationException($"No redemption form found for service '{service}'.");
        }

        var headers = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("utf8", "✓")
        };

        foreach (var input in targetForm.QuerySelectorAll("input"))
        {
            var name = input.GetAttribute("name");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (string.Equals(name, "utf8", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = input.GetAttribute("value");
            if (value is null)
            {
                continue;
            }

            headers.Add(new KeyValuePair<string, string>(name, value));
        }

        return ToFormUrlEncoded(headers);
    }

    public async Task<string> GetCsrfFromHomeAsync(CancellationToken ct = default)
    {
        return await GetCsrfFromPageAsync("home", ct);
    }

    public async Task<string> GetCsrfFromRewardsAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "rewards");
        req.Headers.Referrer = new Uri("https://shift.gearboxsoftware.com/home");

        using var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();

        var html = await res.Content.ReadAsStringAsync(ct);

        return ExtractCsrfTokenFromHtml(html) ?? throw new InvalidOperationException("CSRF token not found on /rewards.");
    }

    public async Task LoginAsync(string email, string password, string csrfToken, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "sessions");
        req.Headers.Referrer = new Uri("https://shift.gearboxsoftware.com/home");
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["authenticity_token"] = csrfToken,
            ["user[email]"] = email,
            ["user[password]"] = password
        });

        using var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task RedeemAsync(string redeemBody, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "code_redemptions");
        req.Headers.Referrer = new Uri("https://shift.gearboxsoftware.com/rewards");

        req.Content = new StringContent(redeemBody, Encoding.UTF8, "application/x-www-form-urlencoded");

        using var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
    }

    private static string? AngleSharpTokenExtract(string html)
    {
        const string marker = "<meta name=\"csrf-token\" content=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            return null;
        }

        start += marker.Length;
        var span = html.AsSpan(start);
        var end = span.IndexOf('"');
        if (end < 0)
        {
            return null;
        }

        return new string(span[..end]);
    }

    private static string? ExtractCsrfTokenFromHtml(string html)
    {
        return AngleSharpTokenExtract(html);
    }

    private async Task<string> GetCsrfFromPageAsync(string relativePath, CancellationToken ct)
    {
        var html = await _http.GetStringAsync(relativePath, ct);

        return ExtractCsrfTokenFromHtml(html) ?? throw new InvalidOperationException($"CSRF token not found on /{relativePath}.");
    }

    private static string ToFormUrlEncoded(IEnumerable<KeyValuePair<string, string>> kvps)
    {
        var sb = new StringBuilder();
        foreach (var (k, v) in kvps)
        {
            if (sb.Length > 0)
            {
                sb.Append('&');
            }

            sb.Append(WebUtility.UrlEncode(k));
            sb.Append('=');
            sb.Append(WebUtility.UrlEncode(v));
        }

        return sb.ToString();
    }
}