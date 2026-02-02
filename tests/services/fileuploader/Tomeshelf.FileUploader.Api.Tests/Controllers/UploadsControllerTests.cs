using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.FileUploader.Api.Controllers;
using Tomeshelf.FileUploader.Application;
using Tomeshelf.FileUploader.Application.Features.Uploads.Commands;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Api.Tests.Controllers;

public class UploadsControllerTests
{
    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenArchiveIsMissing()
    {
        var handler = A.Fake<ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>>();
        var controller = CreateController(handler, new GoogleDriveOptions());

        var result = await controller.Upload(null!, null, CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("A bundle archive (.zip) file is required.");
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenArchiveIsEmpty()
    {
        var handler = A.Fake<ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>>();
        var controller = CreateController(handler, new GoogleDriveOptions());
        var emptyFile = CreateFormFile(Array.Empty<byte>(), "bundle.zip");

        var result = await controller.Upload(emptyFile, null, CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("A bundle archive (.zip) file is required.");
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenCredentialsMissingAndDefaultsUnset()
    {
        var handler = A.Fake<ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>>();
        var controller = CreateController(handler, new GoogleDriveOptions());
        var file = CreateFormFile(new byte[] { 1, 2, 3 }, "bundle.zip");

        var result = await controller.Upload(file, null, CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("Google Drive OAuth credentials are missing. Authorise via the web app and try again.");
    }

    [Fact]
    public async Task Upload_UsesDefaultOptions_WhenCredentialsMissing()
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

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<UploadsController.BundleUploadResponse>();
        captured.Should().NotBeNull();
        ReferenceEquals(defaults, captured!.OverrideOptions).Should().BeTrue();
    }

    [Fact]
    public async Task Upload_UsesCredentialOverrides_WhenProvided()
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

        result.Result.Should().BeOfType<OkObjectResult>();
        captured.Should().NotBeNull();
        captured!.OverrideOptions.Should().NotBeNull();
        captured.OverrideOptions!.ClientId.Should().Be("override-client");
        captured.OverrideOptions.ClientSecret.Should().Be("override-secret");
        captured.OverrideOptions.RefreshToken.Should().Be("override-refresh");
        captured.OverrideOptions.UserEmail.Should().Be("default@example.com");
        captured.OverrideOptions.RootFolderPath.Should().Be("Root");
        captured.OverrideOptions.RootFolderId.Should().Be("root-id");
        captured.OverrideOptions.SharedDriveId.Should().Be("shared-id");
        captured.OverrideOptions.ApplicationName.Should().Be("Tomeshelf");
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
