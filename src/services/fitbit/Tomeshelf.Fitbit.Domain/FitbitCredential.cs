using System;

namespace Tomeshelf.Fitbit.Domain;

/// <summary>
///     Persisted Fitbit OAuth credentials.
/// </summary>
public sealed class FitbitCredential
{
    public int Id { get; set; } = 1;

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }

    public DateTimeOffset? ExpiresAtUtc { get; set; }
}