using System;
using System.Text.Json;

namespace Tomeshelf.Web.Services;

public sealed class FitbitBackendUnavailableException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FitbitBackendUnavailableException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public FitbitBackendUnavailableException(string message) : base(BuildMessage(message)) { }

    /// <summary>
    ///     Builds the message.
    /// </summary>
    /// <param name="raw">The raw.</param>
    /// <returns>The resulting string.</returns>
    private static string BuildMessage(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "Fitbit service is unavailable. Please try again in a moment.";
        }

        var trimmed = raw.Trim();
        if (trimmed.StartsWith("<", StringComparison.Ordinal))
        {
            return "Fitbit service is unavailable. Please try again in a moment.";
        }

        if (trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            try
            {
                using var document = JsonDocument.Parse(trimmed);
                if ((document.RootElement.ValueKind == JsonValueKind.Object) && document.RootElement.TryGetProperty("message", out var messageElement) && (messageElement.ValueKind == JsonValueKind.String))
                {
                    var parsedMessage = messageElement.GetString();
                    if (!string.IsNullOrWhiteSpace(parsedMessage))
                    {
                        return parsedMessage.Trim();
                    }
                }
            }
            catch (JsonException) { }
        }

        if (trimmed.StartsWith("\"", StringComparison.Ordinal) && trimmed.EndsWith("\"", StringComparison.Ordinal))
        {
            try
            {
                trimmed = JsonSerializer.Deserialize<string>(trimmed) ?? trimmed;
            }
            catch (JsonException) { }
        }

        return trimmed;
    }
}