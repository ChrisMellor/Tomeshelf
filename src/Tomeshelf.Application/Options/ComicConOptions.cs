using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomeshelf.Application.Shared.Options;

public sealed class ComicConOptions
{
    [MinLength(1)]
    public List<Location> ComicCon { get; set; } = [];
}