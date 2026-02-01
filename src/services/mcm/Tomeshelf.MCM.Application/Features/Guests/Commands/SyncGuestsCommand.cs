using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Contracts;

namespace Tomeshelf.MCM.Application.Features.Guests.Commands;

public sealed record SyncGuestsCommand(string EventId) : ICommand<GuestSyncResultDto?>;
