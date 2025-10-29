using System;

namespace Tomeshelf.Domain.Entities.Fitness;

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