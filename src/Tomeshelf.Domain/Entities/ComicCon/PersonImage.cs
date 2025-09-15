namespace Tomeshelf.Domain.Entities.ComicCon;

public class PersonImage
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public string Big { get; set; }
    public string Med { get; set; }
    public string Small { get; set; }
    public string Thumb { get; set; }
}