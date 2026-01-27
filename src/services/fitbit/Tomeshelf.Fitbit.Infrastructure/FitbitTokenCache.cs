using Microsoft.AspNetCore.Http;
using System;
using Tomeshelf.Fitbit.Application.Abstractions.Services;

namespace Tomeshelf.Fitbit.Infrastructure;

public sealed class FitbitTokenCache : IFitbitTokenCache
{
    private const string SessionAccessTokenKey = "fitbit_access_token";
    private const string SessionRefreshTokenKey = "fitbit_refresh_token";
    private const string SessionExpiresAtKey = "fitbit_expires_at";

    private readonly IHttpContextAccessor _httpContextAccessor;
    public FitbitTokenCache(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string AccessToken
    {
        get
        {
            if (TryGetSessionValue(SessionAccessTokenKey, out var sessionToken))
            {
                return sessionToken;
            }

            return null;
        }
    }

    public string RefreshToken
    {
        get
        {
            if (TryGetSessionValue(SessionRefreshTokenKey, out var sessionToken))
            {
                return sessionToken;
            }

            return null;
        }
    }

    public DateTimeOffset? ExpiresAtUtc
    {
        get
        {
            if (TryGetSessionValue(SessionExpiresAtKey, out var sessionValue) && DateTimeOffset.TryParse(sessionValue, out var parsed))
            {
                return parsed;
            }

            return null;
        }
    }

    public void Clear()
    {
        ClearSession();
    }

    public void Update(string accessToken, string refreshToken, DateTimeOffset? expiresAtUtc)
    {
        UpdateSession(accessToken, refreshToken, expiresAtUtc);
    }

    private void UpdateSession(string accessToken, string refreshToken, DateTimeOffset? expiresAtUtc)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null || !session.IsAvailable)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            session.SetString(SessionAccessTokenKey, accessToken);
        }

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            session.SetString(SessionRefreshTokenKey, refreshToken);
        }

        if (expiresAtUtc.HasValue)
        {
            session.SetString(SessionExpiresAtKey, expiresAtUtc.Value.ToString("O"));
        }
        else
        {
            session.Remove(SessionExpiresAtKey);
        }
    }

    private void ClearSession()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null || !session.IsAvailable)
        {
            return;
        }

        session.Remove(SessionAccessTokenKey);
        session.Remove(SessionRefreshTokenKey);
        session.Remove(SessionExpiresAtKey);
    }

    private bool TryGetSessionValue(string key, out string value)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null || !session.IsAvailable)
        {
            value = null;
            return false;
        }

        var stored = session.GetString(key);
        if (string.IsNullOrWhiteSpace(stored))
        {
            value = null;
            return false;
        }

        value = stored;
        return true;
    }
}
