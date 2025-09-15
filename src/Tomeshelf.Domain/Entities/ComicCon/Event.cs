using System;
using System.Collections.Generic;

namespace Tomeshelf.Domain.Entities.ComicCon;

public class Event
{
    public int Id { get; set; }
    public string ExternalId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }

    public ICollection<EventAppearance> Appearances { get; set; } = new List<EventAppearance>();
}