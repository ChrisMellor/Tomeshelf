using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tomeshelf.Application;

/// <summary>
/// JSON converter that reads/writes nullable decimals, allowing flexible string inputs.
/// </summary>
public sealed class NullableFlexibleDecimalConverter : JsonConverter<decimal?>
{
    /// <summary>
    /// Attempts to read a decimal value that may be represented as a number, a currency-like string, or null.
    /// Strips common symbols and thousands separators when parsing strings.
    /// </summary>
    /// <param name="reader">JSON reader positioned at the token to read.</param>
    /// <param name="type">Target type (ignored).</param>
    /// <param name="options">Serializer options.</param>
    /// <returns>The parsed decimal value, or null when absent/invalid.</returns>
    public override decimal? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetDecimal();
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString()?.Trim();
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            s = s.Replace(",", "").Replace("£", "").Replace("$", "").Replace("€", "").Trim();
            decimal? result = decimal.TryParse(s, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out var d) ? d : null;

            return result;
        }

        return null;
    }
    /// <summary>
    /// Writes a decimal value or null to the JSON writer.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">Serializer options.</param>
    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteNumberValue(value.Value);
        }
    }
}
