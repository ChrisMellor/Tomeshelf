using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Mcm;

public sealed class McmEventsConfigViewModel
{
    public IReadOnlyList<McmEventConfigModel> Events { get; init; } = [];

    public McmEventConfigEditorModel Editor { get; init; } = new();

    public string? ErrorMessage { get; init; }
}

