namespace Tomeshelf.Web.Models.DriveAuth;

public sealed class OAuthResultViewModel
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public string? UserEmail { get; init; }

    public int? AccessTokenExpiresIn { get; init; }

    public string? RedirectUrl { get; init; }
}