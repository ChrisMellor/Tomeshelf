using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Abstractions.SHiFT;
using Tomeshelf.Application.Contracts.SHiFT;
using Tomeshelf.Domain.Entities.SHiFT;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.Infrastructure.SHiFT;

/// <summary>
///     Provides methods for retrieving and updating SHiFT account settings, including secure handling of credentials, in
///     the underlying data store.
/// </summary>
/// <remarks>
///     This class is intended for use in scenarios where SHiFT account configuration must be securely stored
///     and accessed, such as for automated service authentication. All password values are encrypted at rest and decrypted
///     only when needed for use. This class is not thread-safe; concurrent access should be managed externally if
///     required.
/// </remarks>
public sealed class ShiftSettingsStore : IShiftSettingsStore
{
    private readonly TomeshelfShiftDbContext _context;
    private readonly IDataProtector _protector;

    /// <summary>
    ///     Initializes a new instance of the ShiftSettingsStore class with the specified database context and data
    ///     protection provider.
    /// </summary>
    /// <param name="context">The database context used to access and manage shift settings data.</param>
    /// <param name="dataProtectionProvider">
    ///     The data protection provider used to create a protector for securing sensitive
    ///     information.
    /// </param>
    public ShiftSettingsStore(TomeshelfShiftDbContext context, IDataProtectionProvider dataProtectionProvider)
    {
        _context = context;
        _protector = dataProtectionProvider.CreateProtector("Tomeshelf.SHiFT.ShiftSettings.Password.v1");
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
    public async Task<int> CreateAsync(ShiftSettingsUpdateRequest request, CancellationToken cancellationToken)
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

        var password = request.Password?.Trim();
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentNullException("password", "Missing password");
        }

        var cleanEmail = request.Email?.Trim();

        if (await _context.ShiftSettings.AnyAsync(x => x.Email == cleanEmail, cancellationToken))
        {
            throw new InvalidOperationException("SHiFT email already exists");
        }

        var data = new SettingsEntity
        {
            Email = cleanEmail,
            DefaultService = request.DefaultService?.Trim(),
            EncryptedPassword = string.IsNullOrWhiteSpace(password)
                ? null
                : _protector.Protect(password),
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
    ///     Asynchronously retrieves the shift settings for the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the shift settings to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="ShiftSettingsDto" /> with
    ///     the shift settings for the specified identifier. If no settings are found, a default instance is returned.
    /// </returns>
    public async Task<ShiftSettingsDto> GetAsync(int id, CancellationToken cancellationToken)
    {
        var row = await _context.ShiftSettings
                                .AsNoTracking()
                                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken) ??
                  new SettingsEntity();

        return new ShiftSettingsDto(row.Id, row.Email, row.DefaultService, !string.IsNullOrWhiteSpace(row.EncryptedPassword), row.UpdatedUtc);
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
    public async Task UpdateAsync(int id, ShiftSettingsUpdateRequest request, CancellationToken cancellationToken)
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
        row.UpdatedUtc = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            row.EncryptedPassword = _protector.Protect(request.Password);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}