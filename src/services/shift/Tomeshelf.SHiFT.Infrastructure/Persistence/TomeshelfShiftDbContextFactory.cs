using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace Tomeshelf.SHiFT.Infrastructure.Persistence;

/// <summary>
///     Provides a design-time factory so EF tooling can create the DbContext without the full host.
/// </summary>
public sealed class TomeshelfShiftDbContextFactory : IDesignTimeDbContextFactory<TomeshelfShiftDbContext>
{
    /// <summary>
    ///     Creates the db context.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>The result of the operation.</returns>
    public TomeshelfShiftDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TomeshelfShiftDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__shiftdb") ?? Environment.GetEnvironmentVariable("ConnectionStrings:shiftdb") ?? "Server=(localdb)\\mssqllocaldb;Database=shiftdb;Trusted_Connection=True;MultipleActiveResultSets=true";

        optionsBuilder.UseSqlServer(connectionString);

        return new TomeshelfShiftDbContext(optionsBuilder.Options);
    }
}