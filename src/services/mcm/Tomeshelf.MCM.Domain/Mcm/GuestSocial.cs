using System;

namespace Tomeshelf.MCM.Domain.Mcm;

public class GuestSocial
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GuestSocial" /> class.
    /// </summary>
    public GuestSocial()
    {
        Id = Guid.NewGuid();
        Twitter = string.Empty;
        Facebook = string.Empty;
        Instagram = string.Empty;
        Imdb = string.Empty;
        YouTube = string.Empty;
        Twitch = string.Empty;
        Snapchat = string.Empty;
        DeviantArt = string.Empty;
        Tumblr = string.Empty;
        Fandom = string.Empty;
        GuestInfoId = Guid.NewGuid();
    }

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