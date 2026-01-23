using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Tomeshelf.Application.Shared.Options;
using Tomeshelf.Domain.Shared.Entities.Fitness;
using Tomeshelf.Infrastructure.Shared.Persistence;

namespace Tomeshelf.Infrastructure.Shared.Fitness;

public sealed class FitbitTokenCache
{
    private readonly ILogger<FitbitTokenCache> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly object _sync = new object();

    private string _accessToken;
    private DateTimeOffset? _expiresAtUtc;
    private string _refreshToken;

    public FitbitTokenCache(IServiceScopeFactory scopeFactory, IOptions<FitbitOptions> options, ILogger<FitbitTokenCache> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        if (!TryLoadFromDatabase())
        {
            var value = options.Value;
            _accessToken = string.IsNullOrWhiteSpace(value.AccessToken)
                ? null
                : value.AccessToken;
            _refreshToken = string.IsNullOrWhiteSpace(value.RefreshToken)
                ? null
                : value.RefreshToken;
            _expiresAtUtc = null;

            if (_accessToken is not null || _refreshToken is not null)
            {
                Persist();
            }
        }
    }

    public string AccessToken
    {
        get
        {
            lock (_sync)
            {
                return _accessToken;
            }
        }
    }

    public string RefreshToken
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

    public void Update(string accessToken, string refreshToken, DateTimeOffset? expiresAtUtc)
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

    private void Persist()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
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

    private bool TryLoadFromDatabase()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
            var credential = dbContext.FitbitCredentials
                                      .AsNoTracking()
                                      .SingleOrDefault();
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
}