namespace Tomeshelf.SHiFT.Application.Abstractions.Security;

/// <summary>
///     Defines methods for securely protecting and unprotecting sensitive string data.
/// </summary>
/// <remarks>
///     Implementations of this interface should ensure that the protection mechanism is cryptographically
///     secure and that the unprotection process reliably restores the original plaintext. Callers should handle any
///     exceptions that may occur during protection or unprotection, such as when invalid or tampered data is
///     provided.
/// </remarks>
public interface ISecretProtector
{
    /// <summary>
    ///     Encrypts the specified plaintext to protect its confidentiality.
    /// </summary>
    /// <remarks>
    ///     Use this method to securely protect sensitive information before storage or transmission.
    ///     Ensure that the encryption key is managed securely to maintain the confidentiality of the protected
    ///     data.
    /// </remarks>
    /// <param name="plaintext">The plaintext string to encrypt. This parameter cannot be null or empty.</param>
    /// <returns>A string containing the encrypted representation of the plaintext. Returns null if encryption fails.</returns>
    string Protect(string plaintext);

    /// <summary>
    ///     Decrypts the specified protected value and returns the original plaintext string.
    /// </summary>
    /// <remarks>
    ///     Ensure that the protected value was generated using the corresponding Protect method to
    ///     guarantee successful decryption.
    /// </remarks>
    /// <param name="protectedValue">The encrypted string that needs to be decrypted. This value must not be null or empty.</param>
    /// <returns>
    ///     The original plaintext string that corresponds to the provided protected value. Returns null if the decryption
    ///     fails.
    /// </returns>
    string Unprotect(string protectedValue);
}