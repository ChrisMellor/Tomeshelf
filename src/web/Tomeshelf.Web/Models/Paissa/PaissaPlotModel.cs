using System;

namespace Tomeshelf.Web.Models.Paissa;

public sealed record PaissaPlotModel(int Ward, int Plot, long Price, int Entries, DateTimeOffset LastUpdatedUtc, bool AllowsPersonal, bool AllowsFreeCompany, bool IsEligibilityUnknown);