namespace Tomeshelf.Application.Contracts.SHiFT;

/// <summary>
///     Represents a redemption option for a service, including its identifying information and associated form data.
/// </summary>
/// <param name="Service">
///     The unique identifier or name of the service to which this redemption option applies. Cannot be
///     null or empty.
/// </param>
/// <param name="Title">The title of the redemption option as displayed to users. Cannot be null or empty.</param>
/// <param name="DisplayName">
///     An optional display name for the redemption option. May be null if no alternate display name
///     is provided.
/// </param>
/// <param name="FormBody">
///     The form body or payload associated with the redemption option, typically used to submit redemption details. Cannot
///     be null or empty.
/// </param>
public sealed record RedemptionOption(string Service, string Title, string? DisplayName, string FormBody);