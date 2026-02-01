using Moq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Queries;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;
using Xunit;

namespace Tomeshelf.Fitbit.Application.Tests.Features.Authorization.Queries;

public class GetFitbitAuthorizationStatusQueryHandlerTests
{
    private readonly Mock<IFitbitTokenCache> _mockTokenCache;
    private readonly GetFitbitAuthorizationStatusQueryHandler _handler;

    public GetFitbitAuthorizationStatusQueryHandlerTests()
    {
        _mockTokenCache = new Mock<IFitbitTokenCache>();
        _handler = new GetFitbitAuthorizationStatusQueryHandler(_mockTokenCache.Object);
    }

    [Fact]
    public async Task Handle_HasAccessTokenAndRefreshToken_ReturnsAuthorizedStatus()
    {
        // Arrange
        _mockTokenCache.Setup(tc => tc.AccessToken).Returns("someAccessToken");
        _mockTokenCache.Setup(tc => tc.RefreshToken).Returns("someRefreshToken");

        var query = new GetFitbitAuthorizationStatusQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.HasAccessToken);
        Assert.True(result.HasRefreshToken);
    }

    [Fact]
    public async Task Handle_NoAccessToken_ReturnsUnauthorizedStatus()
    {
        // Arrange
        _mockTokenCache.Setup(tc => tc.AccessToken).Returns(default(string?));
        _mockTokenCache.Setup(tc => tc.RefreshToken).Returns("someRefreshToken");

        var query = new GetFitbitAuthorizationStatusQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.HasAccessToken);
        Assert.True(result.HasRefreshToken);
    }

    [Fact]
    public async Task Handle_NoRefreshToken_ReturnsNoRefreshStatus()
    {
        // Arrange
        _mockTokenCache.Setup(tc => tc.AccessToken).Returns("someAccessToken");
        _mockTokenCache.Setup(tc => tc.RefreshToken).Returns(default(string?));

        var query = new GetFitbitAuthorizationStatusQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.HasAccessToken);
        Assert.False(result.HasRefreshToken);
    }

    [Fact]
    public async Task Handle_NoTokens_ReturnsFullyUnauthorizedStatus()
    {
        // Arrange
        _mockTokenCache.Setup(tc => tc.AccessToken).Returns(default(string?));
        _mockTokenCache.Setup(tc => tc.RefreshToken).Returns(default(string?));

        var query = new GetFitbitAuthorizationStatusQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.HasAccessToken);
        Assert.False(result.HasRefreshToken);
    }
}
