using Tomeshelf.MCM.Application.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Models;

namespace Tomeshelf.MCM.Application.Features.Events.Commands;

public sealed record UpsertEventCommand(EventConfigModel Model) : ICommand<bool>;
