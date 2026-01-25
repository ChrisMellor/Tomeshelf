using System;
using System.Collections.Generic;
using System.Linq;

namespace Tomeshelf.Paissa.Domain.Entities;

/// <summary>
///     Represents a world within the Paissa application, including its unique identifier, name, and associated districts.
/// </summary>
public sealed record PaissaWorld
{
    public int Id { get; }
    public string Name { get; }
    public IReadOnlyList<PaissaDistrict> Districts { get; }

    private PaissaWorld(int id, string name, IReadOnlyList<PaissaDistrict> districts)
    {
        Id = id;
        Name = name;
        Districts = districts;
    }

    public static PaissaWorld Create(int id, string name, IReadOnlyList<PaissaDistrict> districts)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "World id must be positive.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("World name is required.", nameof(name));
        }

        if (districts is null)
        {
            throw new ArgumentNullException(nameof(districts));
        }

        if (districts.Any(district => district is null))
        {
            throw new ArgumentException("Districts cannot contain null entries.", nameof(districts));
        }

        var safeDistricts = districts.ToList();

        return new PaissaWorld(id, name, safeDistricts);
    }

    public PaissaWorld FilterAcceptingEntryDistricts(bool requireKnownSize)
    {
        var districts = Districts
            .Select(district => district.FilterAcceptingEntryPlots(requireKnownSize))
            .Where(district => district is not null)
            .Select(district => district!)
            .ToList();

        return Create(Id, Name, districts);
    }
}
