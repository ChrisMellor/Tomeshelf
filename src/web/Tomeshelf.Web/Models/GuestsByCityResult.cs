using System.Collections.Generic;
using Tomeshelf.Web.Models.ComicCon;

namespace Tomeshelf.Web.Models;

public sealed record GuestsByCityResult(IReadOnlyList<GuestsGroupModel> Groups, int Total);