using System;

namespace Tomeshelf.Web.Services;

public sealed class FitbitAuthorizationRequiredException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FitbitAuthorizationRequiredException" /> class.
    /// </summary>
    /// <param name="location">The location.</param>
    public FitbitAuthorizationRequiredException(Uri location) : base("Fitbit authorization is required.")
    {
        Location = location;
    }

    public Uri Location { get; }
}