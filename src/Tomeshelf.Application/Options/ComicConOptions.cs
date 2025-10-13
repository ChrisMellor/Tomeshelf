using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomeshelf.Application.Options;

public sealed class ComicConOptions
{
    [MinLength(1)]
    public List<Location> ComicCon { get; set; } =
        [];
}