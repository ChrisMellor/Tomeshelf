using System;
using Tomeshelf.Application.Contracts;

namespace Tomeshelf.Infrastructure.Domains.Guests.Records;

public sealed record PersonProjection
{
    public DateTime CreatedUtc { get; init; }

    public PersonDto Person { get; init; }
}