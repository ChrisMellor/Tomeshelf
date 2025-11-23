using System;
using System.Collections.Generic;
using Tomeshelf.Application.Contracts;

namespace Tomeshelf.Infrastructure.Domains.Guests.Records;

public sealed record GuestsGroupResult
{
    public GuestsGroupResult(DateTime createdDate, IReadOnlyList<PersonDto> items)
    {
        CreatedDate = createdDate;
        Items = items;
    }

    public DateTime CreatedDate { get; init; }

    public IReadOnlyList<PersonDto> Items { get; init; }

    public void Deconstruct(out DateTime createdDate, out IReadOnlyList<PersonDto> items)
    {
        createdDate = CreatedDate;
        items = Items;
    }
}