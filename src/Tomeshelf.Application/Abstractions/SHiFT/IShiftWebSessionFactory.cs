namespace Tomeshelf.Application.Abstractions.SHiFT;

/// <summary>
///     Defines a factory for creating new instances of <see cref="IShiftWebSession" />.
/// </summary>
public interface IShiftWebSessionFactory
{
    /// <summary>
    ///     Creates a new instance of a web session for shift operations.
    /// </summary>
    /// <returns>An <see cref="IShiftWebSession" /> that represents the newly created web session.</returns>
    IShiftWebSession Create();
}