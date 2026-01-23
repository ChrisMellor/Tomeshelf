namespace Tomeshelf.Application.Shared.Options;

/// <summary>
///     Options used to configure Google Drive access for bundle uploads.
/// </summary>
public sealed class GoogleDriveOptions
{
    /// <summary>
    ///     Folder path (created if missing) used as the base for uploaded bundles.
    ///     Defaults to the requested "Bundles/Coding/Humble Bundle" hierarchy.
    /// </summary>
    public string RootFolderPath { get; set; }

    /// <summary>
    ///     Optional existing folder ID to use as the root for uploads (e.g. a shared folder in your personal drive).
    ///     When set, the path above is created inside this folder instead of the Drive root.
    /// </summary>
    public string? RootFolderId { get; set; }

    /// <summary>
    ///     Optional shared drive ID to target (recommended for service accounts). When set, uploads occur in this drive.
    /// </summary>
    public string? SharedDriveId { get; set; }

    /// <summary>
    ///     Application name reported to Google Drive.
    /// </summary>
    public string ApplicationName { get; set; } = "Tomeshelf";

    /// <summary>
    ///     OAuth client ID to use when authenticating via user-delegated OAuth (alternative to service accounts).
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    ///     OAuth client secret to use when authenticating via user-delegated OAuth (alternative to service accounts).
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    ///     Refresh token obtained via OAuth consent (offline access). Required when using OAuth instead of service accounts.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    ///     Optional user hint/identifier when using OAuth (used as the user key in token storage).
    /// </summary>
    public string? UserEmail { get; set; }
}