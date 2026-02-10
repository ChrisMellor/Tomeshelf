using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tomeshelf.FileUploader.Application.Abstractions.Upload;
using Tomeshelf.FileUploader.Infrastructure.Upload;

namespace Tomeshelf.FileUploader.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    ///     Adds the infrastructure services.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<BundleFileOrganiser>();
        builder.Services.AddSingleton<IGoogleDriveClientFactory, GoogleDriveClientFactory>();
        builder.Services.AddScoped<IHumbleBundleUploadService, BundleUploadService>();
    }
}