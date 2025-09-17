using System;
using System.Collections.Generic;

namespace Tomeshelf.Domain.Entities.ComicCon;

public class Person
{
    public int Id { get; set; }
    public string ExternalId { get; set; } = null!;
    public string Uid { get; set; }
    public bool PubliclyVisible { get; set; }

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string AltName { get; set; }
    public string Bio { get; set; }
    public string KnownFor { get; set; }

    public string ProfileUrl { get; set; }
    public string ProfileUrlLabel { get; set; }
    public string VideoLink { get; set; }

    public string Twitter { get; set; }
    public string Facebook { get; set; }
    public string Instagram { get; set; }
    public string YouTube { get; set; }
    public string Twitch { get; set; }
    public string Snapchat { get; set; }
    public string DeviantArt { get; set; }
    public string Tumblr { get; set; }

    public ICollection<PersonImage> Images { get; set; } = new List<PersonImage>();
    public ICollection<PersonCategory> Categories { get; set; } = new List<PersonCategory>();
    public ICollection<EventAppearance> Appearances { get; set; } = new List<EventAppearance>();

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
    public DateTime? RemovedUtc { get; set; }
}