using System.Collections.Generic;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Models;

namespace Tomeshelf.MCM.Application.Features.Events.Queries;

public sealed record GetEventsQuery : IQuery<IReadOnlyList<EventConfigModel>>;