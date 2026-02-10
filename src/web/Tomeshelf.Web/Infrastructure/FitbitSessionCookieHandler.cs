using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Tomeshelf.Web.Infrastructure;

public sealed class FitbitSessionCookieHandler : DelegatingHandler
{
    public const string FitbitSessionCookieName = "tomeshelf.fitbit.session";

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FitbitSessionCookieHandler" /> class.
    /// </summary>
    /// <param name="httpContextAccessor">The http context accessor.</param>
    public FitbitSessionCookieHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    ///     Sends asynchronously.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cookies = _httpContextAccessor.HttpContext?.Request?.Cookies;
        if (cookies is not null && cookies.TryGetValue(FitbitSessionCookieName, out var cookie) && !string.IsNullOrWhiteSpace(cookie))
        {
            request.Headers.Remove("Cookie");
            request.Headers.Add("Cookie", $"{FitbitSessionCookieName}={cookie}");
        }

        return base.SendAsync(request, cancellationToken);
    }
}