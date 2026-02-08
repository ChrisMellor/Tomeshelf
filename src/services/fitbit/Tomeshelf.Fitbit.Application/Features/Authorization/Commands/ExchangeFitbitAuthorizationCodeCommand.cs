using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;

namespace Tomeshelf.Fitbit.Application.Features.Authorization.Commands;

public sealed record ExchangeFitbitAuthorizationCodeCommand(string Code, string State) : ICommand<FitbitAuthorizationExchangeResult>;