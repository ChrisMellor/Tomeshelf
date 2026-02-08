using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Paissa;

public sealed record PaissaSizeGroupModel(string Size, string SizeKey, IReadOnlyList<PaissaPlotModel> Plots);