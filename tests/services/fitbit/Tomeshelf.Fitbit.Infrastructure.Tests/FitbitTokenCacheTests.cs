using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Tomeshelf.Fitbit.Infrastructure;
using Xunit;
using FluentAssertions;

namespace Tomeshelf.Fitbit.Infrastructure.Tests;

public class FitbitTokenCacheTests
{
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<HttpContext> _mockContext;
    private readonly Mock<ISession> _mockSession;
    private readonly FitbitTokenCache _cache;

    public FitbitTokenCacheTests()
    {
        _mockAccessor = new Mock<IHttpContextAccessor>();
        _mockContext = new Mock<HttpContext>();
        _mockSession = new Mock<ISession>();

        _mockAccessor.Setup(a => a.HttpContext).Returns(_mockContext.Object);
        _mockContext.Setup(c => c.Session).Returns(_mockSession.Object);
        _mockSession.Setup(s => s.IsAvailable).Returns(true);

        _cache = new FitbitTokenCache(_mockAccessor.Object);
    }

    [Fact]
    public void AccessToken_WhenInSession_ReturnsValue()
    {
        // Arrange
        var token = "someToken";
        var bytes = Encoding.UTF8.GetBytes(token);
        _mockSession.Setup(s => s.TryGetValue("fitbit_access_token", out bytes)).Returns(true);

        // Act
        var result = _cache.AccessToken;

        // Assert
        result.Should().Be(token);
    }

    [Fact]
    public void AccessToken_WhenNotInSession_ReturnsNull()
    {
        // Arrange
        byte[]? bytes = null;
        _mockSession.Setup(s => s.TryGetValue("fitbit_access_token", out bytes)).Returns(false);

        // Act
        var result = _cache.AccessToken;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Update_SetsValuesInSession()
    {
        // Arrange
        var accessToken = "newAccess";
        var refreshToken = "newRefresh";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        _cache.Update(accessToken, refreshToken, expiresAt);

        // Assert
        _mockSession.Verify(s => s.Set("fitbit_access_token", It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == accessToken)), Times.Once);
        _mockSession.Verify(s => s.Set("fitbit_refresh_token", It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == refreshToken)), Times.Once);
        _mockSession.Verify(s => s.Set("fitbit_expires_at", It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == expiresAt.ToString("O"))), Times.Once);
    }

    [Fact]
    public void Clear_RemovesValuesFromSession()
    {
        // Act
        _cache.Clear();

        // Assert
        _mockSession.Verify(s => s.Remove("fitbit_access_token"), Times.Once);
        _mockSession.Verify(s => s.Remove("fitbit_refresh_token"), Times.Once);
        _mockSession.Verify(s => s.Remove("fitbit_expires_at"), Times.Once);
    }
}
