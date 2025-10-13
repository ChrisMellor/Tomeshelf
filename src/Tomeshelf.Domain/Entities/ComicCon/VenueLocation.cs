using System.Collections.Generic;

namespace Tomeshelf.Domain.Entities.ComicCon;

public class VenueLocation
{
    public int Id { get; set; }

    public string ExternalId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}