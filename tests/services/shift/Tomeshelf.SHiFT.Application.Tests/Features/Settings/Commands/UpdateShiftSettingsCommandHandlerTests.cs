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

public class UpdateShiftSettingsCommandHandlerTests
{
    private readonly Mock<IShiftSettingsRepository> _mockRepository;
    private readonly Mock<ISecretProtector> _mockProtector;
    private readonly Mock<IClock> _mockClock;
    private readonly UpdateShiftSettingsCommandHandler _handler;

    public UpdateShiftSettingsCommandHandlerTests()
    {
        _mockRepository = new Mock<IShiftSettingsRepository>();
        _mockProtector = new Mock<ISecretProtector>();
        _mockClock = new Mock<IClock>();
        _handler = new UpdateShiftSettingsCommandHandler(_mockRepository.Object, _mockProtector.Object, _mockClock.Object);
    }

    [Fact]
    public async Task Handle_EntityNotFound_ReturnsFalse()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(SettingsEntity));

        var command = new UpdateShiftSettingsCommand(1, "test@test.com", "pass", "psn");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var entity = new SettingsEntity { Id = 1 };
        _mockRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockRepository
            .Setup(r => r.EmailExistsAsync("existing@test.com", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateShiftSettingsCommand(1, "existing@test.com", "pass", "psn");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidCommandWithPassword_UpdatesEntityAndReturnsTrue()
    {
        // Arrange
        var entity = new SettingsEntity { Id = 1, Email = "old@test.com" };
        var now = DateTimeOffset.UtcNow;

        _mockRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockRepository
            .Setup(r => r.EmailExistsAsync("new@test.com", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockProtector
            .Setup(p => p.Protect("newpass"))
            .Returns("encrypted");

        _mockClock.Setup(c => c.UtcNow).Returns(now);

        var command = new UpdateShiftSettingsCommand(1, "new@test.com", "newpass", "xbox");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal("new@test.com", entity.Email);
        Assert.Equal("xbox", entity.DefaultService);
        Assert.Equal("encrypted", entity.EncryptedPassword);
        Assert.Equal(now, entity.UpdatedUtc);
        _mockRepository.Verify(r => r.UpdateAsync(1, entity, It.IsAny<CancellationToken>()), Times.Once);
    }
}
