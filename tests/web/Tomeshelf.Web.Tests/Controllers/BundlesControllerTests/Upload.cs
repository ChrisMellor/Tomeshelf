using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Shouldly;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models.Bundles;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.Controllers.BundlesControllerTests;

public class Upload
{
    private const string ClientIdKey = "gd_clientId";
    private const string ClientSecretKey = "gd_clientSecret";
    private const string RefreshTokenKey = "gd_refreshToken";
    private const string UserEmailKey = "gd_userEmail";

    /// <summary>
    ///     Shows upload error when the archive is missing.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task WhenArchiveMissing_ShowsUploadError()
    {
        // Arrange
        var controller = new BundlesController(A.Fake<IBundlesApi>(), A.Fake<IFileUploadsApi>());

        // Act
        var result = await controller.Upload(null, CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<BundleUploadViewModel>();
        model.Error.ShouldBe("Please choose a Humble Bundle zip archive to upload.");
    }

    /// <summary>
    ///     Shows auth error when the auth is missing.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAuthMissing_ShowsAuthError()
    {
        // Arrange
        var controller = CreateController(out _);
        var file = CreateArchive();

        // Act
        var result = await controller.Upload(file, CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<BundleUploadViewModel>();
        model.Error.ShouldBe("Google Drive is not authorised. Please run the OAuth flow first.");
    }

    /// <summary>
    ///     Uploads archive when the auth is present.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAuthPresent_UploadsArchive()
    {
        // Arrange
        var uploadsApi = A.Fake<IFileUploadsApi>();
        var controller = CreateController(out var session, uploadsApi);
        session.SetString(ClientIdKey, "client");
        session.SetString(ClientSecretKey, "secret");
        session.SetString(RefreshTokenKey, "refresh");
        session.SetString(UserEmailKey, "user@example.com");

        var file = CreateArchive();
        var resultModel = new BundleUploadResultModel { BundlesProcessed = 1 };

        A.CallTo(() => uploadsApi.UploadBundleAsync(A<Stream>._, "bundle.zip", A<GoogleDriveAuthModel?>._, A<CancellationToken>._))
         .Returns(resultModel);

        // Act
        var result = await controller.Upload(file, CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<BundleUploadViewModel>();
        model.Result.ShouldBeSameAs(resultModel);
        A.CallTo(() => uploadsApi.UploadBundleAsync(A<Stream>._, "bundle.zip", A<GoogleDriveAuthModel?>.That.Matches(auth => (auth.ClientId == "client") && (auth.ClientSecret == "secret") && (auth.RefreshToken == "refresh") && (auth.UserEmail == "user@example.com")), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    /// <summary>
    ///     Shows authorization error when the tokens are missing.
    /// </summary>
    [Fact]
    public void WhenTokensMissing_ShowsAuthorizationError()
    {
        // Arrange
        var controller = CreateController(out _);

        // Act
        var result = controller.Upload();

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<BundleUploadViewModel>();
        model.Error.ShouldBe("Google Drive is not authorised yet. Run the OAuth flow first.");
    }

    /// <summary>
    ///     Returns error when uploading throws.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task WhenUploadThrows_ReturnsError()
    {
        // Arrange
        var uploadsApi = A.Fake<IFileUploadsApi>();
        var controller = CreateController(out var session, uploadsApi);
        session.SetString(ClientIdKey, "client");
        session.SetString(ClientSecretKey, "secret");
        session.SetString(RefreshTokenKey, "refresh");

        var file = CreateArchive();
        A.CallTo(() => uploadsApi.UploadBundleAsync(A<Stream>._, "bundle.zip", A<GoogleDriveAuthModel?>._, A<CancellationToken>._))
         .Throws(new Exception("boom"));

        // Act
        var result = await controller.Upload(file, CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<BundleUploadViewModel>();
        model.Error.ShouldBe("Upload failed: boom");
    }

    /// <summary>
    ///     Creates the archive.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    private static IFormFile CreateArchive()
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

        return new FormFile(stream, 0, stream.Length, "archive", "bundle.zip");
    }

    /// <summary>
    ///     Creates the controller.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="uploadsApi">The uploads api.</param>
    /// <returns>The result of the operation.</returns>
    private static BundlesController CreateController(out TestSession session, IFileUploadsApi uploadsApi = null)
    {
        session = new TestSession();
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = session });

        return new BundlesController(A.Fake<IBundlesApi>(), uploadsApi ?? A.Fake<IFileUploadsApi>()) { ControllerContext = new ControllerContext { HttpContext = httpContext } };
    }
}