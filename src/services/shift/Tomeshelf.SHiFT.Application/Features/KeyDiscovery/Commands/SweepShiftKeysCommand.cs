using System;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;

namespace Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Commands;

public sealed record SweepShiftKeysCommand(TimeSpan Lookback) : ICommand<ShiftKeySweepResult>;
