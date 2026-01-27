using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Features.Redemption.Models;

namespace Tomeshelf.SHiFT.Infrastructure.Services.External;

/// <summary>
///     Provides a session for interacting with the SHiFT web service, enabling authentication, CSRF token retrieval, and
///     code redemption operations over HTTP.
/// </summary>
/// <remarks>
///     A ShiftWebSession manages cookies and HTTP state for a single logical user session with the SHiFT
///     website. It is not thread-safe and should not be shared across concurrent operations. Dispose the session
///     asynchronously when finished to release underlying resources.
/// </remarks>
public sealed class ShiftWebSession : IAsyncDisposable, IShiftWebSession
{
    public const string HttpClientName = "Shift.WebSession";
    private readonly IBrowsingContext _browsingContext;
    private readonly HttpClient _httpClient;

    /// <summary>
    ///     Initializes a new instance of the ShiftWebSession class, configuring HTTP and browsing context settings for
    ///     interacting with the SHiFT web service.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the HTTP client with appropriate headers, cookie handling, and
    ///     automatic decompression to facilitate communication with the SHiFT service. It also initializes a browsing
    ///     context for HTML parsing and navigation. The session is ready for use immediately after construction.
    /// </remarks>
    public ShiftWebSession(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientName);

        //_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Tomeshelf.SHiFT.Api/1.0");
        //_httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/json");

        var config = Configuration.Default;
        _browsingContext = BrowsingContext.New(config);
    }

    /// <summary>
    ///     Asynchronously releases the resources used by the instance.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();

        return ValueTask.CompletedTask;
    }

    /// <summary>
    ///     Asynchronously builds a list of redemption options for the specified code and service by retrieving and parsing
    ///     the corresponding redemption forms.
    /// </summary>
    /// <param name="code">The code to be redeemed. This value is included in the request to identify the entitlement offer.</param>
    /// <param name="csrfToken">
    ///     The CSRF token to include in the request headers for security validation. Cannot be null or
    ///     empty.
    /// </param>
    /// <param name="service">
    ///     The name of the service for which to retrieve redemption options. The method searches for forms matching this
    ///     service.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     A list of <see cref="RedemptionOption" /> objects representing the available redemption options for the specified
    ///     code and service. The list contains one entry for each matching form found.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if no redemption form is found for the specified service.</exception>
    public async Task<List<RedemptionOption>> BuildRedeemBodyAsync(string code, string csrfToken, string service, CancellationToken cancellationToken = default)
    {
        var url = $"entitlement_offer_codes?code={Uri.EscapeDataString(code)}";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("X-Requested-With", "XMLHttpRequest");
        req.Headers.Add("x-csrf-token", csrfToken);
        req.Headers.Accept.Clear();
        req.Headers.Accept.ParseAdd("*/*");

        using var res = await _httpClient.SendAsync(req, cancellationToken);
        res.EnsureSuccessStatusCode();

        var html = await res.Content.ReadAsStringAsync(cancellationToken);

        var doc = await _browsingContext.OpenAsync(r => r.Content(html), cancellationToken);

        var matchingForms = doc.QuerySelectorAll("form")
                               .Where(f =>
                                {
                                    var svc = f.QuerySelector("input[name='archway_code_redemption[service]']")
                                              ?.GetAttribute("value");

                                    return string.Equals(svc, service, StringComparison.OrdinalIgnoreCase);
                                })
                               .ToArray();

        if (matchingForms.Length == 0)
        {
            throw new InvalidOperationException($"No redemption form found for service '{service}'.");
        }

        var options = new List<RedemptionOption>(matchingForms.Length);

        foreach (var form in matchingForms)
        {
            var fields = ExtractNamedInputs(form);

            fields.TryAdd("utf8", "✓");

            fields.TryGetValue("archway_code_redemption[title]", out var title);

            var body = ToFormUrlEncoded(fields);

            var displayName = form.QuerySelector("button")
                                 ?.TextContent
                                 ?.Trim();

            options.Add(new RedemptionOption(service, title ?? string.Empty, string.IsNullOrWhiteSpace(displayName)
                                                 ? null
                                                 : displayName, body));
        }

        return options;
    }

    /// <summary>
    ///     Asynchronously retrieves the CSRF token from the home page.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the CSRF token as a string.</returns>
    public async Task<string> GetCsrfFromHomeAsync(CancellationToken cancellationToken = default)
    {
        return await GetCsrfFromPageAsync("home", cancellationToken);
    }

    /// <summary>
    ///     Asynchronously retrieves the CSRF token from the Shift rewards page.
    /// </summary>
    /// <remarks>
    ///     This method sends an HTTP GET request to the Shift rewards page and parses the response to
    ///     extract the CSRF token required for authenticated operations. The request uses a referrer header set to the
    ///     Shift home page.
    /// </remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the request.</param>
    /// <returns>A string containing the CSRF token extracted from the rewards page.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the CSRF token cannot be found on the rewards page.</exception>
    public async Task<string> GetCsrfFromRewardsAsync(string csrfToken, string email, string password, CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "rewards");
        req.Headers.Referrer = new Uri("https://shift.gearboxsoftware.com/home");

        //req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        //{
        //    ["authenticity_token"] = csrfToken,
        //    ["user[email]"] = email,
        //    ["user[password]"] = password
        //});

        using var res = await _httpClient.SendAsync(req, cancellationToken);
        res.EnsureSuccessStatusCode();

        var html = await res.Content.ReadAsStringAsync(cancellationToken);

        return ExtractCsrfTokenFromHtml(html) ?? throw new InvalidOperationException("CSRF token not found on /rewards.");
    }

    /// <summary>
    ///     Authenticates a user asynchronously using the specified email, password, and CSRF token.
    /// </summary>
    /// <remarks>
    ///     This method sends a POST request to initiate a user session. If authentication fails, an
    ///     exception is thrown. The method does not return a value upon successful authentication.
    /// </remarks>
    /// <param name="email">The email address of the user to authenticate. Cannot be null or empty.</param>
    /// <param name="password">The password associated with the specified email. Cannot be null or empty.</param>
    /// <param name="csrfToken">
    ///     The CSRF (Cross-Site Request Forgery) token required for the authentication request. Cannot be
    ///     null or empty.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the login operation.</param>
    /// <returns>A task that represents the asynchronous login operation.</returns>
    public async Task LoginAsync(string email, string password, string csrfToken, CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "sessions");
        req.Headers.Referrer = new Uri("https://shift.gearboxsoftware.com/home");
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["authenticity_token"] = csrfToken,
            ["user[email]"] = email,
            ["user[password]"] = password
        });

        using var res = await _httpClient.SendAsync(req, cancellationToken);

        res.EnsureSuccessStatusCode();

        var html = await res.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    ///     Submits a redemption request for a code using the specified request body.
    /// </summary>
    /// <remarks>
    ///     The method throws an exception if the redemption request fails. The request is sent as a POST
    ///     to the 'code_redemptions' endpoint, and the referrer header is set to
    ///     'https://shift.gearboxsoftware.com/rewards'.
    /// </remarks>
    /// <param name="redeemBody">
    ///     The request body to send with the redemption request, formatted as application/x-www-form-urlencoded. Cannot be
    ///     null or empty.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous redemption operation.</returns>
    public async Task RedeemAsync(string redeemBody, CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "code_redemptions");
        req.Headers.Referrer = new Uri("https://shift.gearboxsoftware.com/rewards");

        req.Content = new StringContent(redeemBody, Encoding.UTF8, "application/x-www-form-urlencoded");

        using var res = await _httpClient.SendAsync(req, cancellationToken);

        res.EnsureSuccessStatusCode();
    }

    /// <summary>
    ///     Extracts the value of the CSRF token from the specified HTML string using the AngleSharp-compatible meta tag
    ///     format.
    /// </summary>
    /// <remarks>
    ///     This method looks for a meta tag of the form and extracts the value of the content attribute.
    ///     The search is case-sensitive and expects the exact marker format.
    /// </remarks>
    /// <param name="html">The HTML content to search for a CSRF token meta tag. Cannot be null.</param>
    /// <returns>The value of the CSRF token if found; otherwise, null.</returns>
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

    /// <summary>
    ///     Extracts the value of a CSRF token from the specified HTML content.
    /// </summary>
    /// <param name="html">The HTML markup from which to extract the CSRF token. Cannot be null.</param>
    /// <returns>The value of the CSRF token if found; otherwise, null.</returns>
    private static string? ExtractCsrfTokenFromHtml(string html)
    {
        return AngleSharpTokenExtract(html);
    }

    /// <summary>
    ///     Extracts all named input elements from the specified form and returns their names and values as a dictionary.
    /// </summary>
    /// <remarks>
    ///     Only input elements with a non-empty 'name' attribute and a non-null 'value' attribute are
    ///     included in the result. If multiple inputs share the same name, the last one encountered will determine the
    ///     value in the dictionary. The comparison of input names is case-sensitive.
    /// </remarks>
    /// <param name="form">The form element from which to extract input names and values. Must not be null.</param>
    /// <returns>
    ///     A dictionary containing the names and values of all input elements in the form that have both a non-empty name
    ///     and a non-null value. The dictionary is empty if no such inputs are found.
    /// </returns>
    private static Dictionary<string, string> ExtractNamedInputs(IElement form)
    {
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var input in form.QuerySelectorAll("input"))
        {
            var name = input.GetAttribute("name");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var value = input.GetAttribute("value");
            if (value is null)
            {
                continue;
            }

            dict[name] = value;
        }

        return dict;
    }

    /// <summary>
    ///     Asynchronously retrieves the CSRF token from the HTML content of the specified page.
    /// </summary>
    /// <param name="relativePath">
    ///     The relative path of the page from which to extract the CSRF token. Must not be null or
    ///     empty.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A string containing the CSRF token extracted from the page.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the CSRF token cannot be found in the HTML content of the
    ///     specified page.
    /// </exception>
    private async Task<string> GetCsrfFromPageAsync(string relativePath, CancellationToken cancellationToken)
    {
        var html = await _httpClient.GetStringAsync(relativePath, cancellationToken);

        return ExtractCsrfTokenFromHtml(html) ?? throw new InvalidOperationException($"CSRF token not found on /{relativePath}.");
    }

    /// <summary>
    ///     Converts a collection of key-value pairs into a URL-encoded form data string suitable for HTTP POST requests.
    /// </summary>
    /// <remarks>
    ///     This method encodes both keys and values using percent-encoding as defined by
    ///     application/x-www-form-urlencoded. If the collection is empty, the returned string will also be empty.
    /// </remarks>
    /// <param name="inputFields">The collection of key-value pairs to encode. Both keys and values must be non-null strings.</param>
    /// <returns>
    ///     A URL-encoded string representing the form data, with each key-value pair joined by '&' and each key and value
    ///     encoded for safe transmission in a URL.
    /// </returns>
    private static string ToFormUrlEncoded(IEnumerable<KeyValuePair<string, string>> inputFields)
    {
        var sb = new StringBuilder();
        foreach (var (name, value) in inputFields)
        {
            if (sb.Length > 0)
            {
                sb.Append('&');
            }

            sb.Append(WebUtility.UrlEncode(name));
            sb.Append('=');
            sb.Append(WebUtility.UrlEncode(value));
        }

        return sb.ToString();
    }
}
