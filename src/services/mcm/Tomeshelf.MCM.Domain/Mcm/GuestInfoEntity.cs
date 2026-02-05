using System;

namespace Tomeshelf.MCM.Domain.Mcm;

public class GuestInfoEntity
{
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
