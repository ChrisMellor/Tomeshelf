using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Tomeshelf.Web.Infrastructure;

namespace Tomeshelf.Web.Tests.Infrastructure.FitbitSessionCookieHandlerTests;

public class SendAsync
{
    /// <summary>
    ///     Does not add cookie header when the cookie is missing.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
        inner.LastRequest.ShouldNotBeNull();
        inner.LastRequest!.Headers
             .Contains("Cookie")
             .ShouldBeFalse();
    }

    /// <summary>
    ///     Adds cookie header when the cookie is present.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

        /// <summary>
        ///     Sends asynchronously.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}