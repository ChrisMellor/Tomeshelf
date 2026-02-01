using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Queries;
using Tomeshelf.HumbleBundle.Application.HumbleBundle;
using Xunit;

namespace Tomeshelf.HumbleBundle.Application.Tests.Features.Bundles.Queries;

public class GetBundlesQueryHandlerTests
{
    private readonly Mock<IBundleQueries> _mockQueries;
    private readonly GetBundlesQueryHandler _handler;

    public GetBundlesQueryHandlerTests()
    {
        _mockQueries = new Mock<IBundleQueries>();
        _handler = new GetBundlesQueryHandler(_mockQueries.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ValidQuery_CallsIBundleQueriesAndReturnsResult(bool includeExpired)
    {
        // Arrange
        var expectedBundles = new List<BundleDto>
        {
            new("test-bundle", "games", "bundle", "Test Bundle", "Test", "https://url.com", "tile.jpg", "logo.jpg", "hero.jpg", "desc", null, null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        };

        _mockQueries
            .Setup(q => q.GetBundlesAsync(includeExpired, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBundles);

        var query = new GetBundlesQuery(includeExpired);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedBundles, result);
        _mockQueries.Verify(q => q.GetBundlesAsync(includeExpired, It.IsAny<CancellationToken>()), Times.Once);
    }
}
