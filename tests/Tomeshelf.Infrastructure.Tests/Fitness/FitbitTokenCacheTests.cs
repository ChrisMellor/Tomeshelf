using System;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tomeshelf.Application.Options;
using Tomeshelf.Domain.Entities.Fitness;
using Tomeshelf.Infrastructure.Fitness;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.Infrastructure.Tests.Fitness;

public sealed class FitbitTokenCacheTests
{
    [Fact]
    public void ConstructorLoadsStoredCredentials()
    {
        var dbName = Guid.NewGuid().ToString("N");
        using var provider = BuildServiceProvider(dbName);
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        using (var scope = scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
            context.Database.EnsureCreated();
            context.FitbitCredentials.Add(new FitbitCredential
            {
                    AccessToken = "stored-access",
                    RefreshToken = "stored-refresh",
                    ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(1)
            });
            context.SaveChanges();
        }

        var cache = new FitbitTokenCache(scopeFactory, Options.Create(new FitbitOptions()), NullLogger<FitbitTokenCache>.Instance);

        cache.AccessToken.Should().Be("stored-access");
        cache.RefreshToken.Should().Be("stored-refresh");
        cache.ExpiresAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void ConstructorSeedsDatabaseFromOptionsWhenEmpty()
    {
        var dbName = Guid.NewGuid().ToString("N");
        using var provider = BuildServiceProvider(dbName);
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var options = new FitbitOptions
        {
                AccessToken = "bootstrap-access",
                RefreshToken = "bootstrap-refresh"
        };

        using (var scope = scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
            context.Database.EnsureCreated();
        }

        var cache = new FitbitTokenCache(scopeFactory, Options.Create(options), NullLogger<FitbitTokenCache>.Instance);

        cache.AccessToken.Should().Be("bootstrap-access");
        cache.RefreshToken.Should().Be("bootstrap-refresh");

        using (var scope = scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
            var credential = context.FitbitCredentials.Single();
            credential.AccessToken.Should().Be("bootstrap-access");
            credential.RefreshToken.Should().Be("bootstrap-refresh");
        }
    }

    [Fact]
    public void UpdatePersistsCredentials()
    {
        var dbName = Guid.NewGuid().ToString("N");
        using var provider = BuildServiceProvider(dbName);
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        using (var scope = scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
            context.Database.EnsureCreated();
        }

        var cache = new FitbitTokenCache(scopeFactory, Options.Create(new FitbitOptions()), NullLogger<FitbitTokenCache>.Instance);

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        cache.Update("new-access", "new-refresh", expiresAt);

        cache.AccessToken.Should().Be("new-access");
        cache.RefreshToken.Should().Be("new-refresh");
        cache.ExpiresAtUtc.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));

        using (var scope = scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
            var credential = context.FitbitCredentials.Single();
            credential.AccessToken.Should().Be("new-access");
            credential.RefreshToken.Should().Be("new-refresh");
            credential.ExpiresAtUtc.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));
        }
    }

    [Fact]
    public void ClearRemovesPersistedCredentials()
    {
        var dbName = Guid.NewGuid().ToString("N");
        using var provider = BuildServiceProvider(dbName);
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        using (var scope = scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
            context.Database.EnsureCreated();
        }

        var cache = new FitbitTokenCache(scopeFactory, Options.Create(new FitbitOptions()), NullLogger<FitbitTokenCache>.Instance);
        cache.Update("cached-access", "cached-refresh", DateTimeOffset.UtcNow.AddHours(2));

        cache.Clear();

        cache.AccessToken.Should().BeNull();
        cache.RefreshToken.Should().BeNull();
        cache.ExpiresAtUtc.Should().BeNull();

        using (var scope = scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
            var credential = context.FitbitCredentials.Single();
            credential.AccessToken.Should().BeNull();
            credential.RefreshToken.Should().BeNull();
            credential.ExpiresAtUtc.Should().BeNull();
        }
    }

    private static ServiceProvider BuildServiceProvider(string databaseName)
    {
        var services = new ServiceCollection();
        services.AddDbContext<TomeshelfFitbitDbContext>(options =>
        {
            options.UseInMemoryDatabase(databaseName);
        });

        return services.BuildServiceProvider();
    }
}
