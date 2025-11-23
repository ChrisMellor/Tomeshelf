namespace Tomeshelf.Infrastructure.Domains.Fitness.Records;

internal sealed record AuthorizationState
{
    public AuthorizationState(string codeVerifier, string returnUrl)
    {
        CodeVerifier = codeVerifier;
        ReturnUrl = returnUrl;
    }

    public string CodeVerifier { get; }

    public string ReturnUrl { get; }

    public void Deconstruct(out string codeVerifier, out string returnUrl)
    {
        codeVerifier = CodeVerifier;
        returnUrl = ReturnUrl;
    }
}