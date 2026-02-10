using System;
using System.Text.Json;

namespace Tomeshelf.Fitbit.Application.Exceptions;

public sealed class FitbitBadRequestException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FitbitBadRequestException" /> class.
    /// </summary>
    /// <param name="rawMessage">The raw message.</param>
    public FitbitBadRequestException(string rawMessage) : base(BuildMessage(rawMessage)) { }

    /// <summary>
    ///     Builds the message.
    /// </summary>
    /// <param name="raw">The raw.</param>
    /// <returns>The resulting string.</returns>
    private static string BuildMessage(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "Fitbit rejected the request. Please re-authorize and try again.";
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
            return "Fitbit could not process the request. Please re-authorize and try again.";
        }

        return trimmed;
    }
}