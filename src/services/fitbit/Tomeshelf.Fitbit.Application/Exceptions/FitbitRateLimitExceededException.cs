using System;
using System.Text.Json;

namespace Tomeshelf.Fitbit.Application.Exceptions;

public sealed class FitbitRateLimitExceededException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FitbitRateLimitExceededException" /> class.
    /// </summary>
    /// <param name="rawMessage">The raw message.</param>
    /// <param name="retryAfter">The retry after.</param>
    public FitbitRateLimitExceededException(string rawMessage, TimeSpan? retryAfter) : base(BuildMessage(rawMessage))
    {
        RetryAfter = retryAfter;
    }

    public TimeSpan? RetryAfter { get; }

    /// <summary>
    ///     Builds the message.
    /// </summary>
    /// <param name="raw">The raw.</param>
    /// <returns>The resulting string.</returns>
    private static string BuildMessage(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "Fitbit rate limit reached. Please try again shortly.";
        }

        var trimmed = raw.Trim();

        if (trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            try
            {
                using var document = JsonDocument.Parse(trimmed);
                if ((document.RootElement.ValueKind == JsonValueKind.Object) && document.RootElement.TryGetProperty("message", out var messageElement) && (messageElement.ValueKind == JsonValueKind.String))
                {
                    var message = messageElement.GetString();
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        return message.Trim();
                    }
                }
            }
            catch (JsonException) { }
        }

        if (trimmed.StartsWith("\"", StringComparison.Ordinal) && trimmed.EndsWith("\"", StringComparison.Ordinal))
        {
            try
            {
                var deserialised = JsonSerializer.Deserialize<string>(trimmed);
                if (!string.IsNullOrWhiteSpace(deserialised))
                {
                    return deserialised.Trim();
                }
            }
            catch (JsonException) { }
        }

        if (trimmed.StartsWith("<", StringComparison.Ordinal))
        {
            return "Fitbit rate limit reached. Please try again shortly.";
        }

        return trimmed;
    }
}