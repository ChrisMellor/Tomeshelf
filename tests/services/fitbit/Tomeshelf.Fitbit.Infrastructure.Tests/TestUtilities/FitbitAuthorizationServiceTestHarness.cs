using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Infrastructure;

namespace Tomeshelf.Fitbit.Infrastructure.Tests.TestUtilities;

public static class FitbitAuthorizationServiceTestHarness
{
    /// <summary>
    ///     Creates the session context.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public static DefaultHttpContext CreateSessionContext()
    {
        var context = new DefaultHttpContext();
        var session = new TestSession();
        context.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });

        return context;
    }

    /// <summary>
    ///     Creates the service.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="httpContext">The http context.</param>
    /// <param name="cache">The cache.</param>
    /// <param name="client">The client.</param>
    /// <returns>The result of the operation.</returns>
    public static FitbitAuthorizationService CreateService(FitbitOptions options, DefaultHttpContext httpContext, IMemoryCache cache, HttpClient? client = null)
    {
        var httpClient = client ?? new HttpClient(new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)))
        {
            BaseAddress = new Uri("https://api.fitbit.com/")
        };

        var factory = new StubHttpClientFactory(httpClient);
        var tokenCache = new FitbitTokenCache(new HttpContextAccessor { HttpContext = httpContext });

        return new FitbitAuthorizationService(factory, tokenCache, cache, NullLogger<FitbitAuthorizationService>.Instance, new TestOptionsMonitor<FitbitOptions>(options), new HttpContextAccessor { HttpContext = httpContext });
    }

    /// <summary>
    ///     Creates the success client.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns>The result of the operation.</returns>
    public static HttpClient CreateSuccessClient(string payload)
    {
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        });

        return new HttpClient(handler) { BaseAddress = new Uri("https://api.fitbit.com/") };
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StubHttpClientFactory" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public StubHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        ///     Creates the client.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The result of the operation.</returns>
        public HttpClient CreateClient(string name)
        {
            return _client;
        }
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StubHttpMessageHandler" /> class.
        /// </summary>
        /// <param name="response">The response.</param>
        public StubHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        /// <summary>
        ///     Sends asynchronously.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;

        public string Id => "test";

        public IEnumerable<string> Keys => _store.Keys;

        /// <summary>
        ///     Clears.
        /// </summary>
        public void Clear() => _store.Clear();

        /// <summary>
        ///     Commits asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        ///     Loads asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        ///     Removes.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(string key) => _store.Remove(key);

        /// <summary>
        ///     Sets.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(string key, byte[] value) => _store[key] = value;

        /// <summary>
        ///     Attempts to get a value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if the condition is met; otherwise, false.</returns>
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }

    private sealed class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = default!;
    }

    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TestOptionsMonitor" /> class.
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        public TestOptionsMonitor(T currentValue)
        {
            CurrentValue = currentValue;
        }

        public T CurrentValue { get; private set; }

        /// <summary>
        ///     Gets.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The result of the operation.</returns>
        public T Get(string? name) => CurrentValue;

        /// <summary>
        ///     Ons the change.
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <returns>The result of the operation.</returns>
        public IDisposable OnChange(Action<T, string> listener) => new ActionDisposable();

        private sealed class ActionDisposable : IDisposable
        {
            /// <summary>
            ///     Releases resources used by this instance.
            /// </summary>
            public void Dispose() { }
        }
    }
}
