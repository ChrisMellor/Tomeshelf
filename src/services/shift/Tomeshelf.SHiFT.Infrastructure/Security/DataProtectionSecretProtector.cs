using Microsoft.AspNetCore.DataProtection;
using Tomeshelf.SHiFT.Application.Abstractions.Security;

namespace Tomeshelf.SHiFT.Infrastructure.Security;

/// <summary>
///     Provides methods to protect and unprotect sensitive data using data protection mechanisms.
/// </summary>
/// <remarks>
///     This class implements the ISecretProtector interface, allowing for the encryption and decryption of
///     plaintext values. It utilizes an IDataProtector instance created from a specified data protection provider,
///     ensuring
///     that sensitive information is securely handled.
/// </remarks>
public sealed class DataProtectionSecretProtector : ISecretProtector
{
    private readonly IDataProtector _protector;

    /// <summary>
    ///     Initializes a new instance of the DataProtectionSecretProtector class using the specified data protection
    ///     provider.
    /// </summary>
    /// <remarks>
    ///     This constructor creates a data protector with a specific purpose string to ensure that
    ///     protected data is isolated to the intended use case. Use this class to manage the protection and unprotection of
    ///     sensitive data such as passwords.
    /// </remarks>
    /// <param name="provider">
    ///     The data protection provider used to create a data protector for securing sensitive information.
    ///     Cannot be null.
    /// </param>
    public DataProtectionSecretProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Tomeshelf.SHiFT.ShiftSettings.Password.v1");
    }

    /// <summary>
    ///     Encrypts the specified plaintext string to protect sensitive information.
    /// </summary>
    /// <remarks>
    ///     Use this method to securely protect sensitive data before storage or transmission. Ensure
    ///     that the corresponding decryption mechanism is available to recover the original plaintext when
    ///     needed.
    /// </remarks>
    /// <param name="plaintext">The plaintext string to be encrypted. This value cannot be null.</param>
    /// <returns>A string containing the encrypted representation of the provided plaintext.</returns>
    public string Protect(string plaintext)
    {
        return _protector.Protect(plaintext);
    }

    /// <summary>
    ///     Decrypts the specified protected value and returns the original string.
    /// </summary>
    /// <remarks>
    ///     This method relies on the underlying protection mechanism to decrypt the value. Ensure that
    ///     the protected value was created using the corresponding protect method.
    /// </remarks>
    /// <param name="protectedValue">The encrypted string that needs to be decrypted. This value must not be null or empty.</param>
    /// <returns>The original string that was protected. Returns null if the provided protected value is invalid.</returns>
    public string Unprotect(string protectedValue)
    {
        return _protector.Unprotect(protectedValue);
    }
}