namespace Tomeshelf.Web.Models.Bundles;

public sealed class GoogleDriveAuthModel
{
    public string? ClientId { get; init; }

    public string? ClientSecret { get; init; }

    public string? RefreshToken { get; init; }

    public string? UserEmail { get; init; }
}