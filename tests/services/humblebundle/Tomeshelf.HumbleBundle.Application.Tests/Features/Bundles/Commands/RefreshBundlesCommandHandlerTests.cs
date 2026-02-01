using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.HumbleBundle.Application.Abstractions.External;
using Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Commands;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;
using Xunit;

namespace Tomeshelf.HumbleBundle.Application.Tests.Features.Bundles.Commands;

public class RefreshBundlesCommandHandlerTests
{
    private readonly Mock<IHumbleBundleScraper> _mockScraper;
    private readonly Mock<IBundleIngestService> _mockIngestService;
    private readonly RefreshBundlesCommandHandler _handler;

    public RefreshBundlesCommandHandlerTests()
    {
        _mockScraper = new Mock<IHumbleBundleScraper>();
        _mockIngestService = new Mock<IBundleIngestService>();
        _handler = new RefreshBundlesCommandHandler(_mockScraper.Object, _mockIngestService.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsScraperAndIngestService()
    {
        // Arrange
        var scrapedBundles = new List<ScrapedBundle>
        {
            new("test-bundle", "games", "bundle", "Test Bundle", "Test", "https://url.com", "tile.jpg", "logo.jpg", "hero.jpg", "desc", null, null, DateTimeOffset.UtcNow)
        };
        var expectedResult = new BundleIngestResult(1, 0, 0, 1, DateTimeOffset.UtcNow);

        _mockScraper
            .Setup(s => s.ScrapeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(scrapedBundles);

        _mockIngestService
            .Setup(s => s.UpsertAsync(scrapedBundles, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new RefreshBundlesCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockScraper.Verify(s => s.ScrapeAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockIngestService.Verify(s => s.UpsertAsync(scrapedBundles, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ScraperThrowsException_ExceptionIsPropagated()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Scraper failed.");
        _mockScraper
            .Setup(s => s.ScrapeAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var command = new RefreshBundlesCommand();

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(expectedException.Message, thrownException.Message);
        _mockIngestService.Verify(s => s.UpsertAsync(It.IsAny<IReadOnlyList<ScrapedBundle>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
