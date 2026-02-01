using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Features.Settings.Queries;
using Tomeshelf.SHiFT.Domain.Entities;
using Xunit;

namespace Tomeshelf.SHiFT.Application.Tests.Features.Settings.Queries;

public class GetShiftSettingsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenMissing_ReturnsNull()
    {
        // Arrange
        var repository = new Mock<IShiftSettingsRepository>();
        repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((SettingsEntity)null);

        var handler = new GetShiftSettingsQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(new GetShiftSettingsQuery(1), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenEntityExists_MapsToDto()
    {
        // Arrange
        var entity = new SettingsEntity
        {
            Id = 2,
            Email = "user@example.com",
            DefaultService = "psn",
            EncryptedPassword = "encrypted",
            UpdatedUtc = DateTimeOffset.UtcNow
        };

        var repository = new Mock<IShiftSettingsRepository>();
        repository.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(entity);

        var handler = new GetShiftSettingsQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(new GetShiftSettingsQuery(2), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result!.Id);
        Assert.Equal(entity.Email, result.Email);
        Assert.Equal(entity.DefaultService, result.DefaultService);
        Assert.True(result.HasPassword);
        Assert.Equal(entity.UpdatedUtc, result.UpdatedUtc);
    }
}
