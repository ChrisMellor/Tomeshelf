using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Tomeshelf.Web.Infrastructure;

namespace Tomeshelf.Web.Tests.Infrastructure.FitbitSessionCookieHandlerTests;

public class SendAsync
{
    [Fact]
    public async Task WhenCookieMissing_DoesNotAddCookieHeader()
    {
        var context = new DefaultHttpContext();
        var accessor = new HttpContextAccessor { HttpContext = context };
        var inner = new CapturingHandler();
        var handler = new FitbitSessionCookieHandler(accessor) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");

        inner.LastRequest.ShouldNotBeNull();
        inner.LastRequest!.Headers
             .Contains("Cookie")
             .ShouldBeFalse();
    }

    [Fact]
    public async Task WhenCookiePresent_AddsCookieHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{FitbitSessionCookieHandler.FitbitSessionCookieName}=abc";
        var accessor = new HttpContextAccessor { HttpContext = context };
        var inner = new CapturingHandler();
        var handler = new FitbitSessionCookieHandler(accessor) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");

        inner.LastRequest.ShouldNotBeNull();
        inner.LastRequest!.Headers
             .TryGetValues("Cookie", out var values)
             .ShouldBeTrue();
        var value = values.ShouldHaveSingleItem();
        value.ShouldBe($"{FitbitSessionCookieHandler.FitbitSessionCookieName}=abc");
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}