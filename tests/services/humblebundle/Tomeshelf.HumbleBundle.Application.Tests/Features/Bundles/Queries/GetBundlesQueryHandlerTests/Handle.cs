using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Queries;
using Tomeshelf.HumbleBundle.Application.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Application.Tests.Features.Bundles.Queries.GetBundlesQueryHandlerTests;

public class Handle
{
    /// <summary>
    ///     Callsis bundle queries and returns result when the query is valid.
    /// </summary>
    /// <param name="includeExpired">The include expired.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ValidQuery_CallsIBundleQueriesAndReturnsResult(bool includeExpired)
    {
        // Arrange
        var faker = new Faker();
        var queries = A.Fake<IBundleQueries>();
        var handler = new GetBundlesQueryHandler(queries);
        var now = faker.Date.RecentOffset();
        var bundle = new BundleDto(faker.Random.Word(), faker.Random.Word(), faker.Random.Word(), faker.Commerce.ProductName(), faker.Company.CompanyName(), faker.Internet.Url(), faker.Internet.Url(), faker.Internet.Url(), faker.Internet.Url(), faker.Lorem.Sentence(), faker.Date.RecentOffset(), faker.Date.SoonOffset(), now, now, now, now);

        IReadOnlyList<BundleDto> expectedBundles = new List<BundleDto> { bundle };

        A.CallTo(() => queries.GetBundlesAsync(includeExpired, A<CancellationToken>._))
         .Returns(Task.FromResult(expectedBundles));

        var query = new GetBundlesQuery(includeExpired);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedBundles);
        A.CallTo(() => queries.GetBundlesAsync(includeExpired, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}