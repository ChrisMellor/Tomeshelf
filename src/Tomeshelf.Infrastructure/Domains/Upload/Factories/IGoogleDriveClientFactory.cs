using Tomeshelf.Application.Options;
using Tomeshelf.Infrastructure.Domains.Upload.Clients;

namespace Tomeshelf.Infrastructure.Domains.Upload.Factories;

public interface IGoogleDriveClientFactory
{
    IGoogleDriveClient Create(GoogleDriveOptions options);
}