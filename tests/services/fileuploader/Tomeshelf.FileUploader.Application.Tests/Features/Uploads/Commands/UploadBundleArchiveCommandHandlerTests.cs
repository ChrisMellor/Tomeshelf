using Moq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.FileUploader.Application.Abstractions.Upload;
using Tomeshelf.FileUploader.Application.Features.Uploads.Commands;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;
using Xunit;

namespace Tomeshelf.FileUploader.Application.Tests.Features.Uploads.Commands;

public class UploadBundleArchiveCommandHandlerTests
{
    private readonly Mock<IHumbleBundleUploadService> _mockUploadService;
    private readonly UploadBundleArchiveCommandHandler _handler;

    public UploadBundleArchiveCommandHandlerTests()
    {
        _mockUploadService = new Mock<IHumbleBundleUploadService>();
        _handler = new UploadBundleArchiveCommandHandler(_mockUploadService.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUploadServiceAndReturnsResult()
    {
        // Arrange
        var archiveStream = new MemoryStream();
        var fileName = "test.zip";
        GoogleDriveOptions? overrideOptions = null;
        var cancellationToken = CancellationToken.None;
        var expectedResult = BundleUploadResult.FromBooks(new List<BookUploadResult>(), DateTimeOffset.UtcNow);

        var command = new UploadBundleArchiveCommand(archiveStream, fileName, overrideOptions);

        _mockUploadService
            .Setup(s => s.UploadAsync(archiveStream, fileName, overrideOptions, cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        _mockUploadService.Verify(s => s.UploadAsync(archiveStream, fileName, overrideOptions, cancellationToken), Times.Once);
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Handle_UploadServiceThrowsException_ExceptionIsPropagated()
    {
        // Arrange
        var archiveStream = new MemoryStream();
        var fileName = "error.zip";
        GoogleDriveOptions? overrideOptions = null;
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Upload failed.");

        var command = new UploadBundleArchiveCommand(archiveStream, fileName, overrideOptions);

        _mockUploadService
            .Setup(s => s.UploadAsync(archiveStream, fileName, overrideOptions, cancellationToken))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, cancellationToken));
        Assert.Equal(expectedException.Message, thrownException.Message);
        _mockUploadService.Verify(s => s.UploadAsync(archiveStream, fileName, overrideOptions, cancellationToken), Times.Once);
    }
}
