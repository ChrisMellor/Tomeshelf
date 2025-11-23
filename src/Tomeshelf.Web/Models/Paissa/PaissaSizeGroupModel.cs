using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Paissa;

public sealed record PaissaSizeGroupModel
{
    public PaissaSizeGroupModel(string size, string sizeKey, IReadOnlyList<PaissaPlotModel> plots)
    {
        Size = size;
        SizeKey = sizeKey;
        Plots = plots;
    }

    public string Size { get; init; }

    public string SizeKey { get; init; }

    public IReadOnlyList<PaissaPlotModel> Plots { get; init; }

    public void Deconstruct(out string size, out string sizeKey, out IReadOnlyList<PaissaPlotModel> plots)
    {
        size = Size;
        sizeKey = SizeKey;
        plots = Plots;
    }
}