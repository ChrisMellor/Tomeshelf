using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.FileUploader.Api.Controllers;
using Tomeshelf.FileUploader.Application;
using Tomeshelf.FileUploader.Application.Features.Uploads.Commands;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Api.Tests.Controllers.UploadsControllerTests;

public class Upload
{
    [Fact]
    public async Task ReturnsBadRequest_WhenArchiveIsEmpty()
    {
        var handler = A.Fake<ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>>();
        var controller = CreateController(handler, new GoogleDriveOptions());
        var emptyFile = CreateFormFile(Array.Empty<byte>(), "bundle.zip");

        var result = await controller.Upload(emptyFile, null, CancellationToken.None);

        var badRequest = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        badRequest.Value.ShouldBe("A bundle archive (.zip) file is required.");
    }

    [Fact]
    public async Task ReturnsBadRequest_WhenArchiveIsMissing()
    {
        var handler = A.Fake<ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>>();
        var controller = CreateController(handler, new GoogleDriveOptions());

        var result = await controller.Upload(null!, null, CancellationToken.None);

        var badRequest = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        badRequest.Value.ShouldBe("A bundle archive (.zip) file is required.");
    }

    [Fact]
    public async Task ReturnsBadRequest_WhenCredentialsMissingAndDefaultsUnset()
    {
        var handler = A.Fake<ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>>();
        var controller = CreateController(handler, new GoogleDriveOptions());
        var file = CreateFormFile(new byte[] { 1, 2, 3 }, "bundle.zip");

        var result = await controller.Upload(file, null, CancellationToken.None);

        var badRequest = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        badRequest.Value.ShouldBe("Google Drive OAuth credentials are missing. Authorise via the web app and try again.");
    }

    [Fact]
    public async Task UsesCredentialOverrides_WhenProvided()
    {
        var defaults = new GoogleDriveOptions
        {
            RootFolderPath = "Root",
            RootFolderId = "root-id",
            SharedDriveId = "shared-id",
            ApplicationName = "Tomeshelf",
            ClientId = "default-client",
            ClientSecret = "default-secret",
            RefreshToken = "default-refresh",
            UserEmail = "default@example.com"
        };

        var handler = A.Fake<ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>>();
        UploadBundleArchiveCommand? captured = null;
        var expectedResult = BundleUploadResult.FromBooks(new List<BookUploadResult>(), DateTimeOffset.UtcNow);

        A.CallTo(() => handler.Handle(A<UploadBundleArchiveCommand>._, A<CancellationToken>._))
         .Invokes(call => captured = call.GetArgument<UploadBundleArchiveCommand>(0))
         .Returns(Task.FromResult(expectedResult));

        var controller = CreateController(handler, defaults);
        var file = CreateFormFile(new byte[] { 1, 2, 3 }, "bundle.zip");
        var credentials = new UploadsController.OAuthCredentials
        {
            ClientId = "override-client",
            ClientSecret = "override-secret",
            RefreshToken = "override-refresh"
        };

        var result = await controller.Upload(file, credentials, CancellationToken.None);

        result.Result.ShouldBeOfType<OkObjectResult>();
        captured.ShouldNotBeNull();
        captured!.OverrideOptions.ShouldNotBeNull();
        captured.OverrideOptions!.ClientId.ShouldBe("override-client");
        captured.OverrideOptions.ClientSecret.ShouldBe("override-secret");
        captured.OverrideOptions.RefreshToken.ShouldBe("override-refresh");
        captured.OverrideOptions.UserEmail.ShouldBe("default@example.com");
        captured.OverrideOptions.RootFolderPath.ShouldBe("Root");
        captured.OverrideOptions.RootFolderId.ShouldBe("root-id");
        captured.OverrideOptions.SharedDriveId.ShouldBe("shared-id");
        captured.OverrideOptions.ApplicationName.ShouldBe("Tomeshelf");
    }

    [Fact]
    public async Task UsesDefaultOptions_WhenCredentialsMissing()
    {
        var defaults = new GoogleDriveOptions
        {
            RootFolderPath = "Root",
            ClientId = "default-client",
            ClientSecret = "default-secret",
            RefreshToken = "default-refresh",
            UserEmail = "default@example.com"
        };

        var handler = A.Fake<ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>>();
        UploadBundleArchiveCommand? captured = null;
        var expectedResult = BundleUploadResult.FromBooks(new List<BookUploadResult>(), DateTimeOffset.UtcNow);

        A.CallTo(() => handler.Handle(A<UploadBundleArchiveCommand>._, A<CancellationToken>._))
         .Invokes(call => captured = call.GetArgument<UploadBundleArchiveCommand>(0))
         .Returns(Task.FromResult(expectedResult));

        var controller = CreateController(handler, defaults);
        var file = CreateFormFile(new byte[] { 1, 2, 3 }, "bundle.zip");

        var result = await controller.Upload(file, null, CancellationToken.None);

        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeOfType<UploadsController.BundleUploadResponse>();
        captured.ShouldNotBeNull();
        ReferenceEquals(defaults, captured!.OverrideOptions)
           .ShouldBeTrue();
    }

    private static UploadsController CreateController(ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult> handler, GoogleDriveOptions defaults)
    {
        var options = Options.Create(defaults);

        return new UploadsController(handler, options, NullLogger<UploadsController>.Instance);
    }

    private static IFormFile CreateFormFile(byte[] data, string fileName)
    {
        var stream = new MemoryStream(data);

        return new FormFile(stream, 0, data.Length, "archive", fileName);
    }
}