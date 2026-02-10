using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Shift;

public sealed class ShiftIndexViewModel
{
    public string Code { get; init; } = string.Empty;

    public RedeemResponseModel? Response { get; init; }

    public string? ErrorMessage { get; init; }

    public bool HasResponse => Response is not null;

    public IReadOnlyList<ShiftAccountModel> Accounts { get; init; } = [];

    public ShiftAccountEditorModel Account { get; init; } = new();

    public string? AccountErrorMessage { get; init; }
}
