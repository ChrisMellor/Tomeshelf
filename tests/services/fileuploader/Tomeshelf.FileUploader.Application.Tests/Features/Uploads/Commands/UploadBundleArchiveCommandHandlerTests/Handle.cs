using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.FileUploader.Application.Abstractions.Upload;
using Tomeshelf.FileUploader.Application.Features.Uploads.Commands;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Application.Tests.Features.Uploads.Commands.UploadBundleArchiveCommandHandlerTests;

public class Handle
{
    /// <summary>
    ///     The exception is propagated when uploading the service throws.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadServiceThrows_ExceptionIsPropagated()
    {
        // Arrange
        var faker = new Faker();
        var uploadService = A.Fake<IHumbleBundleUploadService>();
        var handler = new UploadBundleArchiveCommandHandler(uploadService);
        using var archiveStream = new MemoryStream();
        var fileName = $"{faker.Random.Word()}.zip";
        GoogleDriveOptions? overrideOptions = null;
        var expectedException = new InvalidOperationException(faker.Lorem.Sentence());
        var command = new UploadBundleArchiveCommand(archiveStream, fileName, overrideOptions);

        A.CallTo(() => uploadService.UploadAsync(archiveStream, fileName, overrideOptions, A<CancellationToken>._))
         .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(act);
        exception.Message.ShouldBe(expectedException.Message);
        A.CallTo(() => uploadService.UploadAsync(archiveStream, fileName, overrideOptions, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    /// <summary>
    ///     Calls upload service and returns result when the command is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ValidCommand_CallsUploadServiceAndReturnsResult()
    {
        // Arrange
        var faker = new Faker();
        var uploadService = A.Fake<IHumbleBundleUploadService>();
        var handler = new UploadBundleArchiveCommandHandler(uploadService);
        using var archiveStream = new MemoryStream();
        var fileName = $"{faker.Random.Word()}.zip";
        GoogleDriveOptions? overrideOptions = null;
        var expectedResult = BundleUploadResult.FromBooks(new List<BookUploadResult>(), faker.Date.RecentOffset());
        var command = new UploadBundleArchiveCommand(archiveStream, fileName, overrideOptions);

        A.CallTo(() => uploadService.UploadAsync(archiveStream, fileName, overrideOptions, A<CancellationToken>._))
         .Returns(Task.FromResult(expectedResult));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeSameAs(expectedResult);
        A.CallTo(() => uploadService.UploadAsync(archiveStream, fileName, overrideOptions, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}