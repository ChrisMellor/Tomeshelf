using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tomeshelf.SHiFT.Application.Features.KeyDiscovery;

public static class ShiftKeyMatcher
{
    private static readonly Regex ShiftKeyRegex = new Regex(@"\b[A-Z0-9]{5}(?:-[A-Z0-9]{5}){4}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    ///     Extracts.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The result of the operation.</returns>
    public static IReadOnlyList<string> Extract(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var matches = ShiftKeyRegex.Matches(text);
        if (matches.Count == 0)
        {
            return Array.Empty<string>();
        }

        var results = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            if (!match.Success)
            {
                continue;
            }

            results.Add(match.Value.ToUpperInvariant());
        }

        return results;
    }
}