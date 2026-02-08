using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Paissa;

public sealed record PaissaDistrictModel(int Id, string Name, IReadOnlyList<PaissaSizeGroupModel> Tabs);