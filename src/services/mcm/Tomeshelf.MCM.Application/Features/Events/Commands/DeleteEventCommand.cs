using Tomeshelf.MCM.Application.Abstractions.Messaging;

namespace Tomeshelf.MCM.Application.Features.Events.Commands;

public sealed record DeleteEventCommand(string EventId) : ICommand<bool>;
