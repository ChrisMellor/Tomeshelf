using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Tomeshelf.MCM.Infrastructure.Tests.TomeshelfMcmDbContextFactoryTests;

public class CreateDbContext : IDisposable
{
    private readonly string? _originalColonEnv;
    private readonly string? _originalUnderscoreEnv;

    public CreateDbContext()
    {
        _originalUnderscoreEnv = Environment.GetEnvironmentVariable("ConnectionStrings__mcmdb");
        _originalColonEnv = Environment.GetEnvironmentVariable("ConnectionStrings:mcmdb");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__mcmdb", _originalUnderscoreEnv);
        Environment.SetEnvironmentVariable("ConnectionStrings:mcmdb", _originalColonEnv);
    }

    [Fact]
    public void UsesColonConnectionString_WhenSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ConnectionStrings__mcmdb", null);
        Environment.SetEnvironmentVariable("ConnectionStrings:mcmdb", "Server=colon_server;Database=colon_db;");
        var factory = new TomeshelfMcmDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        context.ShouldNotBeNull();
        context.Database
               .IsSqlServer()
               .ShouldBeTrue();
        var connectionString = context.Database.GetConnectionString();
        connectionString.ShouldContain("Data Source=colon_server");
        connectionString.ShouldContain("Initial Catalog=colon_db");
    }

    [Fact]
    public void UsesDefaultConnectionString_WhenEnvironmentMissing()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ConnectionStrings__mcmdb", null);
        Environment.SetEnvironmentVariable("ConnectionStrings:mcmdb", null);
        var factory = new TomeshelfMcmDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        context.ShouldNotBeNull();
        context.Database
               .IsSqlServer()
               .ShouldBeTrue();
        var connectionString = context.Database.GetConnectionString();
        connectionString.ShouldContain("Data Source=(localdb)\\mssqllocaldb");
        connectionString.ShouldContain("Initial Catalog=mcmdb");
    }

    [Fact]
    public void UsesUnderscoreConnectionString_WhenSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ConnectionStrings__mcmdb", "Server=underscore_server;Database=underscore_db;");
        Environment.SetEnvironmentVariable("ConnectionStrings:mcmdb", null);
        var factory = new TomeshelfMcmDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        context.ShouldNotBeNull();
        context.Database
               .IsSqlServer()
               .ShouldBeTrue();
        var connectionString = context.Database.GetConnectionString();
        connectionString.ShouldContain("Data Source=underscore_server");
        connectionString.ShouldContain("Initial Catalog=underscore_db");
    }
}