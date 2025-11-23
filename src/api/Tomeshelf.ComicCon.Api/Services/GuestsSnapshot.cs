using System;
using System.Collections.Generic;
using Tomeshelf.Infrastructure.Queries;

namespace Tomeshelf.ComicCon.Api.Services;

/// <summary>
///     Immutable snapshot of guests data for a city.
/// </summary>
public sealed record GuestsSnapshot
{
    /// <summary>
    ///     Immutable snapshot of guests data for a city.
    /// </summary>
    /// <param name="city">City key for the snapshot.</param>
    /// <param name="total">Total number of guests across all groups.</param>
    /// <param name="groups">Date-grouped results.</param>
    /// <param name="generatedUtc">When the snapshot was generated (UTC).</param>
    public GuestsSnapshot(string city, int total, IReadOnlyList<GuestsGroupResult> groups, DateTimeOffset generatedUtc)
    {
        City = city;
        Total = total;
        Groups = groups;
        GeneratedUtc = generatedUtc;
    }

    /// <summary>City key for the snapshot.</summary>
    public string City { get; init; }

    /// <summary>Total number of guests across all groups.</summary>
    public int Total { get; init; }

    /// <summary>Date-grouped results.</summary>
    public IReadOnlyList<GuestsGroupResult> Groups { get; init; }

    /// <summary>When the snapshot was generated (UTC).</summary>
    public DateTimeOffset GeneratedUtc { get; init; }

    public void Deconstruct(out string city, out int total, out IReadOnlyList<GuestsGroupResult> groups, out DateTimeOffset generatedUtc)
    {
        city = City;
        total = Total;
        groups = Groups;
        generatedUtc = GeneratedUtc;
    }
}