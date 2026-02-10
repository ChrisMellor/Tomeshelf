using System.ComponentModel.DataAnnotations;

namespace Tomeshelf.Web.Models.Mcm;

public sealed class McmEventConfigEditorModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Name { get; set; } = string.Empty;
}

