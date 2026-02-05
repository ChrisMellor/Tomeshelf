using System;

namespace Tomeshelf.MCM.Domain.Mcm;

public class GuestSocial
{
    public Guid Id { get; set; }

    public string? Twitter { get; set; }

    public string? Facebook { get; set; }

    public string? Instagram { get; set; }

    public string? Imdb { get; set; }

    public string? YouTube { get; set; }

    public string? Twitch { get; set; }

    public string? Snapchat { get; set; }

    public string? DeviantArt { get; set; }

    public string? Tumblr { get; set; }

    public string? Fandom { get; set; }

    public Guid GuestInfoId { get; set; }

    public GuestInfoEntity? GuestInfo { get; set; }
}