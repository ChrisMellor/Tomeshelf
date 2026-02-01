using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Commands;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;
using Xunit;

namespace Tomeshelf.Fitbit.Application.Tests.Features.Authorization.Commands;

public class BuildFitbitAuthorizationRedirectCommandHandlerTests
{
    private readonly Mock<IFitbitAuthorizationService> _mockAuthorizationService;
    private readonly BuildFitbitAuthorizationRedirectCommandHandler _handler;

    public BuildFitbitAuthorizationRedirectCommandHandlerTests()
    {
        _mockAuthorizationService = new Mock<IFitbitAuthorizationService>();
        _handler = new BuildFitbitAuthorizationRedirectCommandHandler(_mockAuthorizationService.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsAuthorizationRedirect()
    {
        // Arrange
        var returnUrl = "https://example.com/callback";
        var expectedUri = new Uri("https://fitbit.com/oauth2/authorize?code=abc");
        var expectedState = "random_state";

        _mockAuthorizationService
            .Setup(s => s.BuildAuthorizationUri(returnUrl, out It.Ref<string>.IsAny))
            .Returns((string url, out string state) =>
            {
                state = expectedState;
                return expectedUri;
            });

        var command = new BuildFitbitAuthorizationRedirectCommand(returnUrl);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUri, result.AuthorizationUri);
        Assert.Equal(expectedState, result.State);
        _mockAuthorizationService.Verify(s => s.BuildAuthorizationUri(returnUrl, out It.Ref<string>.IsAny), Times.Once);
    }

    [Fact]
    public async Task Handle_BuildAuthorizationUriThrowsException_ExceptionIsPropagated()
    {
        // Arrange
        var returnUrl = "https://example.com/callback";
        var expectedException = new InvalidOperationException("Failed to build URI.");

        _mockAuthorizationService
            .Setup(s => s.BuildAuthorizationUri(returnUrl, out It.Ref<string>.IsAny))
            .Throws(expectedException);

        var command = new BuildFitbitAuthorizationRedirectCommand(returnUrl);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(expectedException.Message, thrownException.Message);
        _mockAuthorizationService.Verify(s => s.BuildAuthorizationUri(returnUrl, out It.Ref<string>.IsAny), Times.Once);
    }
}
