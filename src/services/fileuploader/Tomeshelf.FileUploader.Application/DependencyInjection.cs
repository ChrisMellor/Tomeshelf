using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.FileUploader.Application.Abstractions.Messaging;
using Tomeshelf.FileUploader.Application.Features.Uploads.Commands;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>, UploadBundleArchiveCommandHandler>();

        return services;
    }
}
