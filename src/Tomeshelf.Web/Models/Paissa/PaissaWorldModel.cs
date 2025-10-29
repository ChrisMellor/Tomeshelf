using System;
using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Paissa;

public sealed record PaissaWorldModel(int WorldId, string WorldName, DateTimeOffset RetrievedAtUtc, IReadOnlyList<PaissaDistrictModel> Districts);

public sealed record PaissaDistrictModel(int Id, string Name, IReadOnlyList<PaissaSizeGroupModel> Tabs);

public sealed record PaissaSizeGroupModel(string Size, string SizeKey, IReadOnlyList<PaissaPlotModel> Plots);

public sealed record PaissaPlotModel(int Ward, int Plot, long Price, int Entries, DateTimeOffset LastUpdatedUtc, bool AllowsPersonal, bool AllowsFreeCompany, bool IsEligibilityUnknown);