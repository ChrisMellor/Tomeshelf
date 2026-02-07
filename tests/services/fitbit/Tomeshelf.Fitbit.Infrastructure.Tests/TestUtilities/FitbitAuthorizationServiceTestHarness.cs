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
    public static DefaultHttpContext CreateSessionContext()
    {
        var context = new DefaultHttpContext();
        var session = new TestSession();
        context.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });

        return context;
    }

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

        public StubHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name)
        {
            return _client;
        }
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StubHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

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

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }

    private sealed class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = default!;
    }

    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        public TestOptionsMonitor(T currentValue)
        {
            CurrentValue = currentValue;
        }

        public T CurrentValue { get; private set; }

        public T Get(string? name) => CurrentValue;

        public IDisposable OnChange(Action<T, string> listener) => new ActionDisposable();

        private sealed class ActionDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
