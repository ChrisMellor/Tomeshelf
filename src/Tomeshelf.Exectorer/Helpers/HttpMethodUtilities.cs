namespace Tomeshelf.Executor.Helpers;

/// <summary>
///     Shared HTTP method helper functions.
/// </summary>
internal static class HttpMethodUtilities
{
    public static string Normalise(string? method)
    {
        if (string.IsNullOrWhiteSpace(method))
        {
            return HttpMethod.Get.Method;
        }

        return method.Trim()
                     .ToUpperInvariant();
    }

    public static HttpMethod Resolve(string method)
    {
        return method switch
        {
                "GET" => HttpMethod.Get,
                "POST" => HttpMethod.Post,
                "PUT" => HttpMethod.Put,
                "DELETE" => HttpMethod.Delete,
                "PATCH" => HttpMethod.Patch,
                "HEAD" => HttpMethod.Head,
                "OPTIONS" => HttpMethod.Options,
                "TRACE" => HttpMethod.Trace,
                _ => new HttpMethod(method)
        };
    }
}