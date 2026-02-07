using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Tomeshelf.FileUploader.Application.Abstractions.Upload;
using Tomeshelf.FileUploader.Infrastructure.Upload;

namespace Tomeshelf.FileUploader.Infrastructure.Tests.DependencyInjectionTests;

public class AddInfrastructureServices
{
    [Fact]
    public void RegistersExpectedServices()
    {
        var builder = new HostApplicationBuilder();

        builder.AddInfrastructureServices();

        using var provider = builder.Services.BuildServiceProvider();

        provider.GetRequiredService<BundleFileOrganiser>()
                .ShouldNotBeNull();
        provider.GetRequiredService<IGoogleDriveClientFactory>()
                .ShouldBeOfType<GoogleDriveClientFactory>();
        provider.GetRequiredService<IHumbleBundleUploadService>()
                .ShouldBeOfType<BundleUploadService>();
    }
}