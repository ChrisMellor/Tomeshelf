using System;

namespace Tomeshelf.Fitbit.Application.Abstractions.Services;

public interface IFitbitTokenCache
{
    string AccessToken { get; }

    string RefreshToken { get; }

    DateTimeOffset? ExpiresAtUtc { get; }
}
