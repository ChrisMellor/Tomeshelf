using System;

namespace Tomeshelf.AppHost.Records;

public sealed record ComicConSite
{
    public ComicConSite(string city, Guid key)
    {
        City = city;
        Key = key;
    }

    public string City { get; init; }

    public Guid Key { get; init; }

    public void Deconstruct(out string city, out Guid key)
    {
        city = City;
        key = Key;
    }
}