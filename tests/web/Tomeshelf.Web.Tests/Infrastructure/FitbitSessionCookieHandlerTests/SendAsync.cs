using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Tomeshelf.Web.Infrastructure;

namespace Tomeshelf.Web.Tests.Infrastructure.FitbitSessionCookieHandlerTests;

public class SendAsync
{
    [Fact]
    public async Task WhenCookiePresent_AddsCookieHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{FitbitSessionCookieHandler.FitbitSessionCookieName}=abc";
        var accessor = new HttpContextAccessor { HttpContext = context };
        var inner = new CapturingHandler();
        var handler = new FitbitSessionCookieHandler(accessor) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        // Act
        await client.GetAsync("https://example.test/");

        // Assert
        inner.LastRequest.Should().NotBeNull();
        inner.LastRequest!.Headers.TryGetValues("Cookie", out var values).Should().BeTrue();
        values.Should().ContainSingle().Which.Should().Be($"{FitbitSessionCookieHandler.FitbitSessionCookieName}=abc");
    }

    [Fact]
    public async Task WhenCookieMissing_DoesNotAddCookieHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var accessor = new HttpContextAccessor { HttpContext = context };
        var inner = new CapturingHandler();
        var handler = new FitbitSessionCookieHandler(accessor) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        // Act
        await client.GetAsync("https://example.test/");

        // Assert
        inner.LastRequest.Should().NotBeNull();
        inner.LastRequest!.Headers.Contains("Cookie").Should().BeFalse();
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
