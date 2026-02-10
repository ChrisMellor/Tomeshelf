using System;

namespace Tomeshelf.MCM.Domain.Mcm;

public class GuestInfoEntity
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GuestInfoEntity" /> class.
    /// </summary>
    public GuestInfoEntity()
    {
        Id = Guid.NewGuid();
        FirstName = string.Empty;
        LastName = string.Empty;
        Bio = string.Empty;
        KnownFor = string.Empty;
        Category = string.Empty;
        DaysAppearing = string.Empty;
        ImageUrl = string.Empty;
        Socials = new GuestSocial();
        GuestId = Guid.NewGuid();
    }

    public Guid Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Bio { get; set; }

    public string? KnownFor { get; set; }

    public string? Category { get; set; }

    public string? DaysAppearing { get; set; }

    public string? ImageUrl { get; set; }

    public GuestSocial? Socials { get; set; }

    public Guid GuestId { get; set; }

    public GuestEntity? Guest { get; set; }
}
