using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Paissa;

public sealed record PaissaDistrictModel
{
    public PaissaDistrictModel(int id, string name, IReadOnlyList<PaissaSizeGroupModel> tabs)
    {
        Id = id;
        Name = name;
        Tabs = tabs;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public IReadOnlyList<PaissaSizeGroupModel> Tabs { get; init; }

    public void Deconstruct(out int id, out string name, out IReadOnlyList<PaissaSizeGroupModel> tabs)
    {
        id = Id;
        name = Name;
        tabs = Tabs;
    }
}