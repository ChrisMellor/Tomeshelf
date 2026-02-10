using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.FileUploader.Application.Features.Uploads.Commands;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Application;

public static class DependencyInjection
{
    /// <summary>
    ///     Adds the application services.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>The result of the operation.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>, UploadBundleArchiveCommandHandler>();

        return services;
    }
}