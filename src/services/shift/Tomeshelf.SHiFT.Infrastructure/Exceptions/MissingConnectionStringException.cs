using System;

namespace Tomeshelf.SHiFT.Infrastructure.Exceptions;

/// <summary>
///     Represents an exception that is thrown when a required connection string is missing or invalid.
/// </summary>
/// <remarks>
///     This exception indicates that the application attempted to use a connection string that was not
///     properly defined or was invalid. The exception message includes the specific connection string that caused the
///     error, which can assist in diagnosing configuration issues.
/// </remarks>
public class MissingConnectionStringException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the MissingConnectionStringException class with the specified connection string
    ///     that caused the exception.
    /// </summary>
    /// <remarks>
    ///     This exception indicates that the provided connection string does not meet the required
    ///     format or criteria for a valid connection string. Ensure that the connection string is correctly structured and
    ///     contains all necessary components before passing it to methods or constructors that require it.
    /// </remarks>
    /// <param name="connectionString">
    ///     The connection string that is considered invalid and resulted in this exception being thrown.
    /// </param>
    public MissingConnectionStringException(string connectionString) : base($"The connection string: {connectionString} is invalid") { }
}