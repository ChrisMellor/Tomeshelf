using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
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
    ///     Asynchronously retrieves the current shift settings.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="ShiftSettingsDto" />
    ///     object with the current shift settings. If no settings are found, returns a default instance with empty or
    ///     default values.
    /// </returns>
    public async Task<ShiftSettingsDto> GetAsync(CancellationToken cancellationToken)
    {
        var row = await _context.ShiftSettings
                                .AsNoTracking()
                                .SingleOrDefaultAsync(x => x.Id == 1, cancellationToken) ??
                  new SettingsEntity();

        return new ShiftSettingsDto(row.Email, row.DefaultService, !string.IsNullOrWhiteSpace(row.EncryptedPassword), row.UpdatedUtc);
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
    public async Task<IReadOnlyList<(string Email, string Password, string Service)>> GetForUseAsync(CancellationToken cancellationToken)
    {
        var rows = await _context.ShiftSettings
                                 .AsNoTracking()
                                 .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            throw new InvalidOperationException("SHiFT settings not configured.");
        }

        var users = new List<(string Email, string Password, string Service)>(rows.Count);

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Email))
            {
                throw new InvalidOperationException("SHiFT email not configured.");
            }

            if (string.IsNullOrWhiteSpace(row.EncryptedPassword))
            {
                throw new InvalidOperationException("SHiFT password not configured.");
            }

            var password = _protector.Unprotect(row.EncryptedPassword);

            users.Add((row.Email, password, row.DefaultService));
        }

        return users;
    }

    /// <summary>
    ///     Creates a new shift settings record or updates the existing one with the specified values asynchronously.
    /// </summary>
    /// <param name="request">
    ///     An object containing the updated shift settings values to be applied. The Email and DefaultService properties
    ///     must not be null, empty, or whitespace.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous upsert operation.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if the Email or DefaultService property of the request is null, empty, or consists only of white-space
    ///     characters.
    /// </exception>
    public async Task UpsertAsync(ShiftSettingsUpdateRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email required.");
        }

        if (string.IsNullOrWhiteSpace(request.DefaultService))
        {
            throw new ArgumentException("DefaultService required.");
        }

        var row = await _context.ShiftSettings.SingleOrDefaultAsync(x => x.Id == 1, cancellationToken);
        if (row is null)
        {
            row = new SettingsEntity();
            _context.ShiftSettings.Add(row);
        }

        row.Email = request.Email.Trim();
        row.DefaultService = request.DefaultService.Trim();
        row.UpdatedUtc = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            row.EncryptedPassword = _protector.Protect(request.Password);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}