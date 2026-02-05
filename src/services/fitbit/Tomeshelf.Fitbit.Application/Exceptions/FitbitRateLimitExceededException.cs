using System;
using System.Text.Json;

namespace Tomeshelf.Fitbit.Application.Exceptions;

public sealed class FitbitRateLimitExceededException : Exception
{
    public FitbitRateLimitExceededException(string rawMessage, TimeSpan? retryAfter) : base(BuildMessage(rawMessage))
    {
        RetryAfter = retryAfter;
    }

    public TimeSpan? RetryAfter { get; }

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
            catch (JsonException)
            {
                // ignore malformed json
            }
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
            catch (JsonException)
            {
                // ignore malformed json
            }
        }

        if (trimmed.StartsWith("<", StringComparison.Ordinal))
        {
            return "Fitbit rate limit reached. Please try again shortly.";
        }

        return trimmed;
    }
}