using Microsoft.EntityFrameworkCore;
using Tomeshelf.SHiFT.Infrastructure.Persistence;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

internal static class ShiftSettingsRepositoryTestHarness
{
    internal static async Task<TomeshelfShiftDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<TomeshelfShiftDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                      .Options;

        var context = new TomeshelfShiftDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return context;
    }

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
