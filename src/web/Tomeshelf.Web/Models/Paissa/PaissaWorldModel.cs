using System;
using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Paissa;

public sealed record PaissaWorldModel(int WorldId, string WorldName, DateTimeOffset RetrievedAtUtc, IReadOnlyList<PaissaDistrictModel> Districts);