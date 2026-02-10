using System.Net.Http;

namespace Tomeshelf.Web.Tests.TestUtilities;

public sealed class TestHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TestHttpClientFactory" /> class.
    /// </summary>
    /// <param name="client">The client.</param>
    public TestHttpClientFactory(HttpClient client)
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