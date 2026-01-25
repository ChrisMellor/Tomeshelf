using Tomeshelf.Paissa.Application.Abstractions.Common;

namespace Tomeshelf.Paissa.Infrastructure.Settings;

/// <summary>
/// Represents the configuration settings for a specific world in the Paissa application.
/// </summary>
/// <param name="WorldId">The unique identifier for the world. Must be a positive integer.</param>
public sealed record PaissaWorldSettings(int WorldId) : IPaissaWorldSettings;
