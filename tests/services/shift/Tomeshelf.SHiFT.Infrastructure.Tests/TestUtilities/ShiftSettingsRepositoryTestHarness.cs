using Microsoft.EntityFrameworkCore;
using Tomeshelf.SHiFT.Infrastructure.Persistence;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

internal static class ShiftSettingsRepositoryTestHarness
{
    /// <summary>
    ///     Creates the context asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    internal static async Task<TomeshelfShiftDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<TomeshelfShiftDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                      .Options;

        var context = new TomeshelfShiftDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return context;
    }

    /// <summary>
    ///     Creates the sqlite context asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    internal static async Task<TomeshelfShiftDbContext> CreateSqliteContextAsync()
    {
        var options = new DbContextOptionsBuilder<TomeshelfShiftDbContext>()
                      .UseSqlite("DataSource=:memory:")
                      .Options;

        var context = new TomeshelfShiftDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        return context;
    }
}
