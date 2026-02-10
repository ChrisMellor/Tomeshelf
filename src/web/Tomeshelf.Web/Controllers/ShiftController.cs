using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Web.Models.Shift;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Controllers;

[Route("shift")]
public sealed class ShiftController(IShiftApi api) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var (accounts, errorMessage) = await LoadAccountsAsync(cancellationToken);

        return View(new ShiftIndexViewModel
        {
            Accounts = accounts,
            AccountErrorMessage = errorMessage
        });
    }

    [HttpPost("redeem")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Redeem([FromForm] ShiftIndexViewModel model, CancellationToken cancellationToken = default)
    {
        var (accounts, accountsError) = await LoadAccountsAsync(cancellationToken);

        var code = model.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            ModelState.AddModelError(nameof(ShiftIndexViewModel.Code), "Enter a SHiFT code to redeem.");

            return View("Index", new ShiftIndexViewModel
            {
                Code = model.Code,
                Accounts = accounts,
                AccountErrorMessage = accountsError
            });
        }

        try
        {
            var response = await api.RedeemCodeAsync(code, cancellationToken);

            return View("Index", new ShiftIndexViewModel
            {
                Code = code,
                Response = response,
                Accounts = accounts,
                AccountErrorMessage = accountsError
            });
        }
        catch (Exception ex)
        {
            return View("Index", new ShiftIndexViewModel
            {
                Code = code,
                ErrorMessage = $"Redeem failed: {ex.Message}",
                Accounts = accounts,
                AccountErrorMessage = accountsError
            });
        }
    }

    [HttpPost("accounts/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAccount([Bind(Prefix = "Account")] ShiftAccountEditorModel account, CancellationToken cancellationToken = default)
    {
        account.Email = (account.Email ?? string.Empty).Trim();
        account.DefaultService = (account.DefaultService ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(account.Email))
        {
            ModelState.AddModelError("Account.Email", "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(account.Password))
        {
            ModelState.AddModelError("Account.Password", "Password is required.");
        }

        if (string.IsNullOrWhiteSpace(account.DefaultService))
        {
            ModelState.AddModelError("Account.DefaultService", "Default service is required.");
        }

        var (accounts, accountsError) = await LoadAccountsAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return View("Index", new ShiftIndexViewModel
            {
                Accounts = accounts,
                AccountErrorMessage = accountsError,
                Account = account
            });
        }

        var created = await api.CreateAccountAsync(account, cancellationToken);
        if (!created)
        {
            ModelState.AddModelError("Account.Email", "An account with this email already exists.");

            return View("Index", new ShiftIndexViewModel
            {
                Accounts = accounts,
                AccountErrorMessage = accountsError,
                Account = account
            });
        }

        TempData["StatusMessage"] = $"Added account '{account.Email}'.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("accounts/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount([FromForm] int id, CancellationToken cancellationToken = default)
    {
        await api.DeleteAccountAsync(id, cancellationToken);

        TempData["StatusMessage"] = $"Deleted account ID {id}.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<(IReadOnlyList<ShiftAccountModel> Accounts, string? ErrorMessage)> LoadAccountsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var accounts = await api.GetAccountsAsync(cancellationToken);

            return (accounts, null);
        }
        catch (Exception ex)
        {
            return (Array.Empty<ShiftAccountModel>(), $"Unable to load SHiFT accounts: {ex.Message}");
        }
    }
}
