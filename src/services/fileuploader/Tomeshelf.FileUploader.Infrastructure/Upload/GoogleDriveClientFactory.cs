using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Tomeshelf.FileUploader.Application;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

internal sealed class GoogleDriveClientFactory : IGoogleDriveClientFactory
{
    private readonly IOptions<GoogleDriveOptions> _defaults;
    private readonly IServiceProvider _serviceProvider;

    public GoogleDriveClientFactory(IOptions<GoogleDriveOptions> defaults, IServiceProvider serviceProvider)
    {
        _defaults = defaults;
        _serviceProvider = serviceProvider;
    }

    public IGoogleDriveClient Create(GoogleDriveOptions options)
    {
        var merged = new GoogleDriveOptions
        {
            ApplicationName = options.ApplicationName ?? _defaults.Value.ApplicationName,
            RootFolderPath = string.IsNullOrWhiteSpace(options.RootFolderPath)
                ? _defaults.Value.RootFolderPath
                : options.RootFolderPath,
            RootFolderId = string.IsNullOrWhiteSpace(options.RootFolderId)
                ? _defaults.Value.RootFolderId
                : options.RootFolderId,
            ClientId = options.ClientId ?? _defaults.Value.ClientId,
            ClientSecret = options.ClientSecret ?? _defaults.Value.ClientSecret,
            RefreshToken = options.RefreshToken ?? _defaults.Value.RefreshToken,
            UserEmail = options.UserEmail ?? _defaults.Value.UserEmail,
            SharedDriveId = options.SharedDriveId ?? _defaults.Value.SharedDriveId
        };

        return ActivatorUtilities.CreateInstance<GoogleDriveClient>(_serviceProvider, Options.Create(merged));
    }
}
