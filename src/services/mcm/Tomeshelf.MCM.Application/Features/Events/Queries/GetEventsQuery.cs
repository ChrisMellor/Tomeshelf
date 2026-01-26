using System.Collections.Generic;
using Tomeshelf.MCM.Application.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Models;

namespace Tomeshelf.MCM.Application.Features.Events.Queries;

public sealed record GetEventsQuery() : IQuery<IReadOnlyList<EventConfigModel>>;
