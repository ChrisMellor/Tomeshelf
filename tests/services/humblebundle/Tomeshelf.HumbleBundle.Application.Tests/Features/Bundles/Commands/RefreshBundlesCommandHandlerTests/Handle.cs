using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.HumbleBundle.Application.Abstractions.External;
using Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Commands;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;

namespace Tomeshelf.HumbleBundle.Application.Tests.Features.Bundles.Commands.RefreshBundlesCommandHandlerTests;

public class Handle
{
    [Fact]
    public async Task ScraperThrowsException_ExceptionIsPropagated()
    {
        // Arrange
        var faker = new Faker();
        var scraper = A.Fake<IHumbleBundleScraper>();
        var ingestService = A.Fake<IBundleIngestService>();
        var handler = new RefreshBundlesCommandHandler(scraper, ingestService);
        var expectedException = new InvalidOperationException(faker.Lorem.Sentence());

        A.CallTo(() => scraper.ScrapeAsync(A<CancellationToken>._))
         .ThrowsAsync(expectedException);

        Func<Task> act = () => handler.Handle(new RefreshBundlesCommand(), CancellationToken.None);

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(act);
        // Assert
        exception.Message.ShouldBe(expectedException.Message);
        A.CallTo(() => ingestService.UpsertAsync(A<IReadOnlyList<ScrapedBundle>>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }

    [Fact]
    public async Task ValidCommand_CallsScraperAndIngestService()
    {
        // Arrange
        var faker = new Faker();
        var scraper = A.Fake<IHumbleBundleScraper>();
        var ingestService = A.Fake<IBundleIngestService>();
        var handler = new RefreshBundlesCommandHandler(scraper, ingestService);

        IReadOnlyList<ScrapedBundle> scrapedBundles = new List<ScrapedBundle> { new(faker.Random.Word(), faker.Random.Word(), faker.Random.Word(), faker.Commerce.ProductName(), faker.Company.CompanyName(), faker.Internet.Url(), faker.Internet.Url(), faker.Internet.Url(), faker.Internet.Url(), faker.Lorem.Sentence(), faker.Date.RecentOffset(), faker.Date.SoonOffset(), faker.Date.RecentOffset()) };

        var expectedResult = new BundleIngestResult(1, 0, 0, 1, faker.Date.RecentOffset());

        A.CallTo(() => scraper.ScrapeAsync(A<CancellationToken>._))
         .Returns(Task.FromResult(scrapedBundles));

        A.CallTo(() => ingestService.UpsertAsync(scrapedBundles, A<CancellationToken>._))
         .Returns(Task.FromResult(expectedResult));

        // Act
        var result = await handler.Handle(new RefreshBundlesCommand(), CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResult);
        A.CallTo(() => scraper.ScrapeAsync(A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => ingestService.UpsertAsync(scrapedBundles, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}