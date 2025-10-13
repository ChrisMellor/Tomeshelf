namespace Tomeshelf.Domain.Entities.ComicCon;

public class PersonCategory
{
    public int PersonId { get; set; }

    public Person Person { get; set; } = null!;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;
}