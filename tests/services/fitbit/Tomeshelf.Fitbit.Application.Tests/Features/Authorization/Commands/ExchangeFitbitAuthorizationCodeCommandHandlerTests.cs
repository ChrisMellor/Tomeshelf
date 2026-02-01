using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Commands;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;
using Xunit;

namespace Tomeshelf.Fitbit.Application.Tests.Features.Authorization.Commands;

public class ExchangeFitbitAuthorizationCodeCommandHandlerTests
{
    private readonly Mock<IFitbitAuthorizationService> _mockAuthorizationService;
    private readonly ExchangeFitbitAuthorizationCodeCommandHandler _handler;

    public ExchangeFitbitAuthorizationCodeCommandHandlerTests()
    {
        _mockAuthorizationService = new Mock<IFitbitAuthorizationService>();
        _handler = new ExchangeFitbitAuthorizationCodeCommandHandler(_mockAuthorizationService.Object);
    }

    [Fact]
    public async Task Handle_TryConsumeStateReturnsFalse_ReturnsErrorResult()
    {
        // Arrange
        var command = new ExchangeFitbitAuthorizationCodeCommand("someCode", "invalidState");
        string codeVerifier = null!;
        string returnUrl = null!;

        _mockAuthorizationService
            .Setup(s => s.TryConsumeState(command.State!, out codeVerifier, out returnUrl))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsInvalidState);
        Assert.Equal("/fitness", result.ReturnUrl);
        _mockAuthorizationService.Verify(s => s.TryConsumeState(command.State!, out codeVerifier, out returnUrl), Times.Once);
        _mockAuthorizationService.Verify(s => s.ExchangeAuthorizationCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TryConsumeStateReturnsTrue_CallsExchangeAndReturnsSuccessResult()
    {
        // Arrange
        var command = new ExchangeFitbitAuthorizationCodeCommand("someCode", "validState");
        var codeVerifier = "someCodeVerifier";
        var returnUrl = "/dashboard";

        _mockAuthorizationService
            .Setup(s => s.TryConsumeState(command.State!, out codeVerifier, out returnUrl))
            .Returns(true);

        _mockAuthorizationService
            .Setup(s => s.ExchangeAuthorizationCodeAsync(command.Code, codeVerifier, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsInvalidState);
        Assert.Equal(returnUrl, result.ReturnUrl);
        _mockAuthorizationService.Verify(s => s.TryConsumeState(command.State!, out codeVerifier, out returnUrl), Times.Once);
        _mockAuthorizationService.Verify(s => s.ExchangeAuthorizationCodeAsync(command.Code, codeVerifier, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ExchangeAuthorizationCodeThrowsException_ExceptionIsPropagated()
    {
        // Arrange
        var command = new ExchangeFitbitAuthorizationCodeCommand("someCode", "validState");
        var codeVerifier = "someCodeVerifier";
        var returnUrl = "/dashboard";
        var expectedException = new InvalidOperationException("Exchange failed.");

        _mockAuthorizationService
            .Setup(s => s.TryConsumeState(command.State!, out codeVerifier, out returnUrl))
            .Returns(true);

        _mockAuthorizationService
            .Setup(s => s.ExchangeAuthorizationCodeAsync(command.Code, codeVerifier, CancellationToken.None))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(expectedException.Message, thrownException.Message);
        _mockAuthorizationService.Verify(s => s.TryConsumeState(command.State!, out codeVerifier, out returnUrl), Times.Once);
        _mockAuthorizationService.Verify(s => s.ExchangeAuthorizationCodeAsync(command.Code, codeVerifier, CancellationToken.None), Times.Once);
    }
}
