using System;

namespace Tomeshelf.Web.Exceptions;

public sealed class FitbitAuthorizationRequiredException : Exception
{
    public FitbitAuthorizationRequiredException(Uri location) : base("Fitbit authorization is required.")
    {
        Location = location;
    }

    public Uri Location { get; }
}