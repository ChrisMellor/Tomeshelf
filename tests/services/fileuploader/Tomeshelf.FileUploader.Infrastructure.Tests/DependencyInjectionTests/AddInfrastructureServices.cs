using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Tomeshelf.FileUploader.Application.Abstractions.Upload;
using Tomeshelf.FileUploader.Infrastructure.Upload;

namespace Tomeshelf.FileUploader.Infrastructure.Tests.DependencyInjectionTests;

public class AddInfrastructureServices
{
    /// <summary>
    ///     Registers the expected services.
    /// </summary>
    [Fact]
    public void RegistersExpectedServices()
    {
        // Arrange
        var builder = new HostApplicationBuilder();

        builder.AddInfrastructureServices();

        // Act
        using var provider = builder.Services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<BundleFileOrganiser>()
                .ShouldNotBeNull();
        provider.GetRequiredService<IGoogleDriveClientFactory>()
                .ShouldBeOfType<GoogleDriveClientFactory>();
        provider.GetRequiredService<IHumbleBundleUploadService>()
                .ShouldBeOfType<BundleUploadService>();
    }
}