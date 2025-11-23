using System;
using System.ComponentModel.DataAnnotations;

namespace Tomeshelf.Application.Options;

public sealed class Location
{
    [Required]
    public string City { get; init; } = string.Empty;

    [Required]
    public Guid Key { get; init; }
}