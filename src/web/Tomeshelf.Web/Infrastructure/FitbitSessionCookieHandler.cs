using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Tomeshelf.Web.Infrastructure;

public sealed class FitbitSessionCookieHandler : DelegatingHandler
{
    public const string FitbitSessionCookieName = "tomeshelf.fitbit.session";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public FitbitSessionCookieHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

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