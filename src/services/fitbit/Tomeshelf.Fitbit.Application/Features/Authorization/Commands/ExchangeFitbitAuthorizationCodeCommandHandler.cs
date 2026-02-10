using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;

namespace Tomeshelf.Fitbit.Application.Features.Authorization.Commands;

public sealed class ExchangeFitbitAuthorizationCodeCommandHandler : ICommandHandler<ExchangeFitbitAuthorizationCodeCommand, FitbitAuthorizationExchangeResult>
{
    private readonly IFitbitAuthorizationService _authorizationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExchangeFitbitAuthorizationCodeCommandHandler" /> class.
    /// </summary>
    /// <param name="authorizationService">The authorization service.</param>
    public ExchangeFitbitAuthorizationCodeCommandHandler(IFitbitAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<FitbitAuthorizationExchangeResult> Handle(ExchangeFitbitAuthorizationCodeCommand command, CancellationToken cancellationToken)
    {
        if (!_authorizationService.TryConsumeState(command.State ?? string.Empty, out var codeVerifier, out var returnUrl))
        {
            return new FitbitAuthorizationExchangeResult(true, "/fitness");
        }

        await _authorizationService.ExchangeAuthorizationCodeAsync(command.Code, codeVerifier, cancellationToken);

        return new FitbitAuthorizationExchangeResult(false, returnUrl);
    }
}