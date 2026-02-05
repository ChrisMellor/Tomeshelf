using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tomeshelf.MCM.Infrastructure;

/// <summary>
///     Provides a design-time factory so EF tooling can create the DbContext without the full host.
/// </summary>
public sealed class TomeshelfMcmDbContextFactory : IDesignTimeDbContextFactory<TomeshelfMcmDbContext>
{
    public TomeshelfMcmDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TomeshelfMcmDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__mcmdb") ?? Environment.GetEnvironmentVariable("ConnectionStrings:mcmdb") ?? "Server=(localdb)\\mssqllocaldb;Database=mcmdb;Trusted_Connection=True;MultipleActiveResultSets=true";

        optionsBuilder.UseSqlServer(connectionString);

        return new TomeshelfMcmDbContext(optionsBuilder.Options);
    }
}