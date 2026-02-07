using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.FileUploader.Application.Abstractions.Upload;
using Tomeshelf.FileUploader.Application.Features.Uploads.Commands;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Application.Tests.Features.Uploads.Commands.UploadBundleArchiveCommandHandlerTests;

public class Handle
{
    [Fact]
    public async Task UploadServiceThrows_ExceptionIsPropagated()
    {
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

        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        var exception = await Should.ThrowAsync<InvalidOperationException>(act);
        exception.Message.ShouldBe(expectedException.Message);
        A.CallTo(() => uploadService.UploadAsync(archiveStream, fileName, overrideOptions, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ValidCommand_CallsUploadServiceAndReturnsResult()
    {
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

        var result = await handler.Handle(command, CancellationToken.None);

        result.ShouldBeSameAs(expectedResult);
        A.CallTo(() => uploadService.UploadAsync(archiveStream, fileName, overrideOptions, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}