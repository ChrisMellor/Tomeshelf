namespace Tomeshelf.Web.Models;

using ComicCon;
using System.Collections.Generic;

public sealed record GuestsByCityResult(IReadOnlyList<GuestsGroupModel> Groups, int Total, bool WarmingUp);

