using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.FileUploader.Application.Abstractions.Upload;
using Tomeshelf.FileUploader.Application.Features.Uploads.Commands;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Application.Tests.DependencyInjectionTests;

public class AddApplicationServices
{
    [Fact]
    public void RegistersHandlers()
    {
        var services = new ServiceCollection();
        services.AddSingleton(A.Fake<IHumbleBundleUploadService>());

        services.AddApplicationServices();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>>()
                .ShouldBeOfType<UploadBundleArchiveCommandHandler>();
    }
}