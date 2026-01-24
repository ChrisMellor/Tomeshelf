using Tomeshelf.SHiFT.Application.Abstractions.External;

namespace Tomeshelf.SHiFT.Infrastructure.Services.External;

/// <summary>
///     Provides a factory for creating instances of the <see cref="IShiftWebSession" /> interface.
/// </summary>
/// <remarks>
///     This factory is sealed and cannot be inherited. It is designed to create new web session instances as
///     needed.
/// </remarks>
public sealed class ShiftWebSessionFactory : IShiftWebSessionFactory
{
    /// <summary>
    ///     Creates a new instance of the default <see cref="IShiftWebSession" /> implementation.
    /// </summary>
    /// <returns>A fresh <see cref="IShiftWebSession" /> ready for use in the current request context.</returns>
    public IShiftWebSession Create()
    {
        return new ShiftWebSession();
    }
}