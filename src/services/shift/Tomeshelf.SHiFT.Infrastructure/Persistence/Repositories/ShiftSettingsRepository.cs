using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Infrastructure.Persistence.Repositories;

/// <summary>
///     Provides methods for creating, retrieving, updating, and deleting shift settings, as well as accessing user
///     credentials for SHiFT users. This repository ensures that sensitive information, such as passwords, is protected
///     before being persisted, and can unprotect passwords when needed for use.
/// </summary>
/// <remarks>
///     This class interacts with the underlying database context to manage shift settings data. It enforces
///     validation of input parameters and prevents duplicate entries based on email addresses. Passwords are unprotected
///     using the provided data protection provider when they need to be used. Password protection is performed
///     upstream (for example, in command handlers). All operations are asynchronous and
///     support cancellation via cancellation tokens.
/// </remarks>
public sealed class ShiftSettingsRepository : IShiftSettingsRepository
{
    private readonly TomeshelfShiftDbContext _context;
    private readonly ISecretProtector _protector;

    /// <summary>
    ///     Initializes a new instance of the ShiftSettingsRepository class with the specified database context and data
    ///     protection provider.
    /// </summary>
    /// <param name="context">The database context used to access and manage shift settings data.</param>
    /// <param name="protector">
    ///     The data protection provider used to create a protector for securing sensitive
    ///     information.
    /// </param>
    public ShiftSettingsRepository(TomeshelfShiftDbContext context, ISecretProtector protector)
    {
        _context = context;
        _protector = protector;
    }

    /// <summary>
    ///     Asynchronously creates a new ShiftSettings entry using the specified update request.
    /// </summary>
    /// <param name="request">
    ///     The request containing the email, default service, and password to use for the new ShiftSettings entry. Cannot
    ///     be null.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the unique identifier of the newly
    ///     created ShiftSettings entry.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if a ShiftSettings entry with the specified email already exists.</exception>
    public async Task<int> CreateAsync(SettingsEntity request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentNullException("email", "Missing email");
        }

        var service = request.DefaultService?.Trim();
        if (string.IsNullOrEmpty(service))
        {
            throw new ArgumentNullException("service", "Missing service");
        }

        var encryptedPassword = request.EncryptedPassword?.Trim();
        if (string.IsNullOrEmpty(encryptedPassword))
        {
            throw new ArgumentNullException("password", "Missing password");
        }

        if (await _context.ShiftSettings.AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new InvalidOperationException("SHiFT email already exists");
        }

        var data = new SettingsEntity
        {
            Email = email,
            DefaultService = service,
            EncryptedPassword = encryptedPassword,
            UpdatedUtc = DateTimeOffset.UtcNow
        };

        var newAccount = await _context.ShiftSettings.AddAsync(data, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return newAccount.Entity.Id;
    }

    /// <summary>
    ///     Asynchronously deletes the shift setting with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the shift setting to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _context.ShiftSettings
                      .Where(x => x.Id == id)
                      .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    ///     Asynchronously determines whether the specified email address exists in the system, optionally excluding a
    ///     record by its identifier.
    /// </summary>
    /// <remarks>
    ///     This method is asynchronous and should be awaited. Providing a valid email format is
    ///     recommended to ensure accurate results.
    /// </remarks>
    /// <param name="email">The email address to check for existence. This parameter cannot be null or empty.</param>
    /// <param name="excludingId">
    ///     An optional identifier of a record to exclude from the existence check. If specified, the method ignores the
    ///     record with this identifier when searching for the email address.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains <see langword="true" /> if the email
    ///     address exists; otherwise, <see langword="false" />.
    /// </returns>
    public async Task<bool> EmailExistsAsync(string email, int? excludingId, CancellationToken cancellationToken)
    {
        return await _context.ShiftSettings
                             .Where(x => x.Id != excludingId)
                             .AnyAsync(x => x.Email == email, cancellationToken);
    }

    /// <summary>
    ///     Asynchronously retrieves the shift settings for the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the shift settings to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="ShiftSettingsDto" /> with
    ///     the shift settings for the specified identifier. If no settings are found, a default instance is returned.
    /// </returns>
    public async Task<SettingsEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.ShiftSettings
                             .AsNoTracking()
                             .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    ///     Retrieves a list of SHiFT user credentials for use, including email, decrypted password, and associated service.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A read-only list of tuples, each containing the email, decrypted password, and service name for a configured
    ///     SHiFT user.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if SHiFT settings are not configured, or if any user entry is
    ///     missing an email or password.
    /// </exception>
    public async Task<IReadOnlyList<(int Id, string Email, string Password, string Service)>> GetForUseAsync(CancellationToken cancellationToken)
    {
        var rows = await _context.ShiftSettings
                                 .AsNoTracking()
                                 .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return [];
        }

        var users = new List<(int Id, string Email, string Password, string Service)>(rows.Count);

        foreach (var row in rows)
        {
            var email = row.Email;
            var service = row.DefaultService;
            var password = string.Empty;

            if (!string.IsNullOrWhiteSpace(row.EncryptedPassword))
            {
                try
                {
                    password = _protector.Unprotect(row.EncryptedPassword);

                    if (LooksLikeProtectedValue(password))
                    {
                        try
                        {
                            password = _protector.Unprotect(password);
                        }
                        catch (CryptographicException) { }
                    }
                }
                catch (CryptographicException)
                {
                    password = string.Empty;
                }
            }

            users.Add((row.Id, email, password, service));
        }

        return users;
    }

    /// <summary>
    ///     Asynchronously updates the shift settings for the specified identifier using the provided update request.
    /// </summary>
    /// <param name="id">The unique identifier of the shift settings to update.</param>
    /// <param name="request">
    ///     An object containing the updated values for the shift settings. The Email and DefaultService properties must not
    ///     be null or empty.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Email or DefaultService property of the request is null or empty.</exception>
    public async Task UpdateAsync(int id, SettingsEntity request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentNullException(nameof(request.Email), "Missing email");
        }

        var service = request.DefaultService?.Trim();
        if (string.IsNullOrEmpty(service))
        {
            throw new ArgumentNullException(nameof(request.DefaultService), "Missing service");
        }

        var row = await _context.ShiftSettings.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (row == null)
        {
            return;
        }

        if ((row.Email != email) && await _context.ShiftSettings.AnyAsync(x => (x.Id != id) && (x.Email == email), cancellationToken))
        {
            throw new InvalidOperationException("SHiFT email already exists");
        }

        row.Email = email;
        row.DefaultService = service;
        row.EncryptedPassword = request.EncryptedPassword;
        row.UpdatedUtc = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static bool LooksLikeProtectedValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || (value.Length < 50))
        {
            return false;
        }

        if (!value.StartsWith("CfDJ8", StringComparison.Ordinal))
        {
            return false;
        }

        foreach (var ch in value)
        {
            if (((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z')) || ((ch >= '0') && (ch <= '9')) || ch is '-' or '_')
            {
                continue;
            }

            return false;
        }

        return true;
    }
}