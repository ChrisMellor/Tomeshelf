using System.Collections.Generic;

namespace Tomeshelf.Domain.Entities.ComicCon;

public class EventAppearance
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public Event Event { get; set; } = null!;

    public int PersonId { get; set; }

    public Person Person { get; set; } = null!;

    public string DaysAtShow { get; set; }

    public string BoothNumber { get; set; }

    public decimal? AutographAmount { get; set; }

    public decimal? PhotoOpAmount { get; set; }

    public decimal? PhotoOpTableAmount { get; set; }

    public ICollection<Schedule> Schedules { get; set; } =
        [];
}