using Microsoft.EntityFrameworkCore;
using Tomeshelf.HumbleBundle.Domain.HumbleBundle;
using Tomeshelf.HumbleBundle.Infrastructure.Bundles;

namespace Tomeshelf.HumbleBundle.Infrastructure.Tests.Bundles.BundleQueriesTests;

public class GetBundlesAsync
{
    [Fact]
    public async Task ExcludesExpiredAndOrdersByEndDateThenTitle()
    {
        // Arrange
        await using var context = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var past = now.AddHours(-2);
        var soon = now.AddHours(1);
        var later = now.AddHours(4);

        var bundles = new List<Bundle>
        {
            CreateBundle("b1", "Alpha", later),
            CreateBundle("b2", "Beta", soon),
            CreateBundle("b3", "Gamma", null),
            CreateBundle("b4", "Delta", null),
            CreateBundle("b5", "Epsilon", past)
        };

        context.Bundles.AddRange(bundles);
        await context.SaveChangesAsync();

        var queries = new BundleQueries(context);
        var before = DateTimeOffset.UtcNow;

        // Act
        var result = await queries.GetBundlesAsync(false, CancellationToken.None);

        // Assert
        var after = DateTimeOffset.UtcNow;
        result.Select(r => r.MachineName).ShouldBe(new[] { "b2", "b1", "b4", "b3" });
        result.ShouldNotContain(r => r.MachineName == "b5");
        result.Select(r => r.GeneratedUtc).Distinct().ShouldHaveSingleItem();
        result[0].GeneratedUtc.ShouldBeInRange(before, after);
    }

    [Fact]
    public async Task IncludesExpiredAndOrdersByEndDateThenTitle()
    {
        // Arrange
        await using var context = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var past = now.AddHours(-2);
        var soon = now.AddHours(1);
        var later = now.AddHours(4);

        var bundles = new List<Bundle>
        {
            CreateBundle("b1", "Alpha", later),
            CreateBundle("b2", "Beta", soon),
            CreateBundle("b3", "Gamma", null),
            CreateBundle("b4", "Delta", null),
            CreateBundle("b5", "Epsilon", past)
        };

        context.Bundles.AddRange(bundles);
        await context.SaveChangesAsync();

        var queries = new BundleQueries(context);
        var before = DateTimeOffset.UtcNow;

        // Act
        var result = await queries.GetBundlesAsync(true, CancellationToken.None);

        // Assert
        var after = DateTimeOffset.UtcNow;
        result.Select(r => r.MachineName).ShouldBe(new[] { "b5", "b2", "b1", "b4", "b3" });
        result.Select(r => r.GeneratedUtc).Distinct().ShouldHaveSingleItem();
        result[0].GeneratedUtc.ShouldBeInRange(before, after);
    }

    private static Bundle CreateBundle(string machineName, string title, DateTimeOffset? endsAt)
    {
        var now = DateTimeOffset.UtcNow;

        return new Bundle
        {
            MachineName = machineName,
            Category = "books",
            Stamp = "bundle",
            Title = title,
            ShortName = title,
            Url = "https://example.com/" + machineName,
            TileImageUrl = "tile",
            TileLogoUrl = "logo",
            HeroImageUrl = "hero",
            ShortDescription = "desc",
            StartsAt = now.AddDays(-1),
            EndsAt = endsAt,
            FirstSeenUtc = now.AddDays(-2),
            LastSeenUtc = now.AddDays(-1),
            LastUpdatedUtc = now.AddDays(-1)
        };
    }

    private static TomeshelfBundlesDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TomeshelfBundlesDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                      .Options;

        return new TomeshelfBundlesDbContext(options);
    }
}
