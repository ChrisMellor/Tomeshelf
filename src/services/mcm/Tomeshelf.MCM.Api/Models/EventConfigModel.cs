namespace Tomeshelf.MCM.Api.Models;

/// <summary>
///     Represents the configuration settings for an event, including its unique identifier and name.
/// </summary>
public class EventConfigModel
{
    /// <summary>
    ///     Gets the unique identifier for this instance.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    ///     Gets the name associated with the object.
    /// </summary>
    public string Name { get; init; }
}