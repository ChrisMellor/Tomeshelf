using System;

namespace Tomeshelf.Domain.Entities.ComicCon;

public class Schedule
{
    public int Id { get; set; }

    public string ExternalId { get; set; } = null!;

    public int EventAppearanceId { get; set; }

    public EventAppearance EventAppearance { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; }

    public DateTime StartTimeUtc { get; set; }

    public DateTime? EndTimeUtc { get; set; }

    public bool NoEndTime { get; set; }

    public string Location { get; set; }

    public int? VenueLocationId { get; set; }

    public VenueLocation VenueLocation { get; set; }
}