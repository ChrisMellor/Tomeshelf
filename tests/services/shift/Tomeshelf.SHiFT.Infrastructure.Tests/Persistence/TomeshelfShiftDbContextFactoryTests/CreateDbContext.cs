using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Persistence;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Persistence.TomeshelfShiftDbContextFactoryTests;

public class CreateDbContext : IDisposable
{
    private readonly string? _originalColonEnv;
    private readonly string? _originalUnderscoreEnv;

    public CreateDbContext()
    {
        _originalUnderscoreEnv = Environment.GetEnvironmentVariable("ConnectionStrings__shiftdb");
        _originalColonEnv = Environment.GetEnvironmentVariable("ConnectionStrings:shiftdb");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__shiftdb", _originalUnderscoreEnv);
        Environment.SetEnvironmentVariable("ConnectionStrings:shiftdb", _originalColonEnv);
    }

    [Fact]
    public void UsesColonConnectionString_WhenSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ConnectionStrings__shiftdb", null);
        Environment.SetEnvironmentVariable("ConnectionStrings:shiftdb", "Server=colon_server;Database=colon_db;");
        var factory = new TomeshelfShiftDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        context.Database
               .IsSqlServer()
               .ShouldBeTrue();
        context.Database
               .GetConnectionString()
               .ShouldContain("Data Source=colon_server");
        context.Database
               .GetConnectionString()
               .ShouldContain("Initial Catalog=colon_db");
    }

    [Fact]
    public void UsesDefaultConnectionString_WhenNoEnvSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ConnectionStrings__shiftdb", null);
        Environment.SetEnvironmentVariable("ConnectionStrings:shiftdb", null);
        var factory = new TomeshelfShiftDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        context.Database
               .IsSqlServer()
               .ShouldBeTrue();
        context.Database
               .GetConnectionString()
               .ShouldContain("Data Source=(localdb)\\mssqllocaldb");
        context.Database
               .GetConnectionString()
               .ShouldContain("Initial Catalog=shiftdb");
    }

    [Fact]
    public void UsesUnderscoreConnectionString_WhenSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ConnectionStrings__shiftdb", "Server=underscore_server;Database=underscore_db;");
        Environment.SetEnvironmentVariable("ConnectionStrings:shiftdb", null);
        var factory = new TomeshelfShiftDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        context.Database
               .IsSqlServer()
               .ShouldBeTrue();
        context.Database
               .GetConnectionString()
               .ShouldContain("Data Source=underscore_server");
        context.Database
               .GetConnectionString()
               .ShouldContain("Initial Catalog=underscore_db");
    }
}