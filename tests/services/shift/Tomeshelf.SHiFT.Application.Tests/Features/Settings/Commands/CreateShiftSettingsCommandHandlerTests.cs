using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;
using Tomeshelf.SHiFT.Domain.Entities;
using Xunit;

namespace Tomeshelf.SHiFT.Application.Tests.Features.Settings.Commands;

public class CreateShiftSettingsCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEmailExists_Throws()
    {
        // Arrange
        var repository = new Mock<IShiftSettingsRepository>();
        var protector = new Mock<ISecretProtector>();
        var clock = new Mock<IClock>();
        var handler = new CreateShiftSettingsCommandHandler(repository.Object, protector.Object, clock.Object);

        repository.Setup(r => r.EmailExistsAsync("user@example.com", null, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

        var command = new CreateShiftSettingsCommand("user@example.com", "secret", "psn");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithEmptyPassword_SetsNullEncryptedPassword()
    {
        // Arrange
        var repository = new Mock<IShiftSettingsRepository>();
        var protector = new Mock<ISecretProtector>();
        var clock = new Mock<IClock>();
        var handler = new CreateShiftSettingsCommandHandler(repository.Object, protector.Object, clock.Object);
        var now = DateTimeOffset.UtcNow;

        repository.Setup(r => r.EmailExistsAsync("user@example.com", null, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(false);
        clock.Setup(c => c.UtcNow).Returns(now);
        repository.Setup(r => r.CreateAsync(It.IsAny<SettingsEntity>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(7);

        var command = new CreateShiftSettingsCommand("user@example.com", "", "psn");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(7, result);
        protector.Verify(p => p.Protect(It.IsAny<string>()), Times.Never);
        repository.Verify(r => r.CreateAsync(It.Is<SettingsEntity>(e =>
            e.Email == "user@example.com" &&
            e.DefaultService == "psn" &&
            e.EncryptedPassword == null &&
            e.UpdatedUtc == now), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPassword_ProtectsAndStoresEncryptedPassword()
    {
        // Arrange
        var repository = new Mock<IShiftSettingsRepository>();
        var protector = new Mock<ISecretProtector>();
        var clock = new Mock<IClock>();
        var handler = new CreateShiftSettingsCommandHandler(repository.Object, protector.Object, clock.Object);
        var now = DateTimeOffset.UtcNow;

        repository.Setup(r => r.EmailExistsAsync("user@example.com", null, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(false);
        clock.Setup(c => c.UtcNow).Returns(now);
        protector.Setup(p => p.Protect("secret")).Returns("encrypted");
        repository.Setup(r => r.CreateAsync(It.IsAny<SettingsEntity>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(9);

        var command = new CreateShiftSettingsCommand("user@example.com", "secret", "steam");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(9, result);
        protector.Verify(p => p.Protect("secret"), Times.Once);
        repository.Verify(r => r.CreateAsync(It.Is<SettingsEntity>(e =>
            e.Email == "user@example.com" &&
            e.DefaultService == "steam" &&
            e.EncryptedPassword == "encrypted" &&
            e.UpdatedUtc == now), It.IsAny<CancellationToken>()), Times.Once);
    }
}
