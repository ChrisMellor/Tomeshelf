using Microsoft.EntityFrameworkCore;

namespace Tomeshelf.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for Tomeshelf's bundle domain entities.
/// </summary>
/// <param name="options">DbContext options configured by DI.</param>
public class TomeshelfBundlesDbContext(DbContextOptions<TomeshelfBundlesDbContext> options) : DbContext(options) { }
