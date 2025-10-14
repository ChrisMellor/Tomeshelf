using System.Collections.Generic;

namespace Tomeshelf.Domain.Entities.ComicCon;

public class Category
{
    public int Id { get; set; }

    public string ExternalId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public ICollection<PersonCategory> PersonLinks { get; set; } = new List<PersonCategory>();
}