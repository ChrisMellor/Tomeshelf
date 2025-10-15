#nullable enable
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tomeshelf.Application.Options;
using Tomeshelf.Domain.Entities.Fitness;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.Infrastructure.Fitness;

public sealed class FitbitTokenCache
{
    private readonly object _sync = new();
    private readonly IDbContextFactory<TomeshelfFitbitDbContext> _dbContextFactory;
    private readonly ILogger<FitbitTokenCache> _logger;

    private string? _accessToken;
    private string? _refreshToken;
    private DateTimeOffset? _expiresAtUtc;

    public FitbitTokenCache(
        IDbContextFactory<TomeshelfFitbitDbContext> dbContextFactory,
        IOptions<FitbitOptions> options,
        ILogger<FitbitTokenCache> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;

        if (!TryLoadFromDatabase())
        {
            var value = options.Value;
            _accessToken = string.IsNullOrWhiteSpace(value.AccessToken) ? null : value.AccessToken;
            _refreshToken = string.IsNullOrWhiteSpace(value.RefreshToken) ? null : value.RefreshToken;
            _expiresAtUtc = null;

            if (_accessToken is not null || _refreshToken is not null)
            {
                Persist();
            }
        }
    }

    public string? AccessToken
    {
        get
        {
            lock (_sync)
            {
                return _accessToken;
            }
        }
    }

    public string? RefreshToken
    {
        get
        {
            lock (_sync)
            {
                return _refreshToken;
            }
        }
    }

    public DateTimeOffset? ExpiresAtUtc
    {
        get
        {
            lock (_sync)
            {
                return _expiresAtUtc;
            }
        }
    }

    public void Update(string accessToken, string? refreshToken, DateTimeOffset? expiresAtUtc)
    {
        lock (_sync)
        {
            _accessToken = accessToken;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                _refreshToken = refreshToken;
            }

            _expiresAtUtc = expiresAtUtc;
            Persist();
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            _accessToken = null;
            _refreshToken = null;
            _expiresAtUtc = null;
            Persist();
        }
    }

    private bool TryLoadFromDatabase()
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var credential = dbContext.FitbitCredentials.AsNoTracking().SingleOrDefault();
            if (credential is null)
            {
                return false;
            }

            _accessToken = credential.AccessToken;
            _refreshToken = credential.RefreshToken;
            _expiresAtUtc = credential.ExpiresAtUtc;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read Fitbit credentials from database.");
            return false;
        }
    }

    private void Persist()
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var credential = dbContext.FitbitCredentials.SingleOrDefault();
            if (credential is null)
            {
                credential = new FitbitCredential();
                dbContext.FitbitCredentials.Add(credential);
            }

            credential.AccessToken = _accessToken;
            credential.RefreshToken = _refreshToken;
            credential.ExpiresAtUtc = _expiresAtUtc;

            dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist Fitbit credentials to database.");
        }
    }
}
