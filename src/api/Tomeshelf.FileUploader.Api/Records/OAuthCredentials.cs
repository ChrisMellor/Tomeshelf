namespace Tomeshelf.FileUploader.Api.Records;

public sealed class OAuthCredentials
{
    public string ClientId { get; init; }

    public string ClientSecret { get; init; }

    public string RefreshToken { get; init; }

    public string UserEmail { get; init; }
}