using System.ComponentModel.DataAnnotations;

namespace Tomeshelf.Web.Models.Shift;

public sealed class ShiftAccountEditorModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string DefaultService { get; set; } = "psn";
}

