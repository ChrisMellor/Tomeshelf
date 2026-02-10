using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;

namespace Tomeshelf.Fitbit.Application.Features.Authorization.Commands;

public sealed class BuildFitbitAuthorizationRedirectCommandHandler : ICommandHandler<BuildFitbitAuthorizationRedirectCommand, FitbitAuthorizationRedirect>
{
    private readonly IFitbitAuthorizationService _authorizationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BuildFitbitAuthorizationRedirectCommandHandler" /> class.
    /// </summary>
    /// <param name="authorizationService">The authorization service.</param>
    public BuildFitbitAuthorizationRedirectCommandHandler(IFitbitAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<FitbitAuthorizationRedirect> Handle(BuildFitbitAuthorizationRedirectCommand command, CancellationToken cancellationToken)
    {
        var uri = _authorizationService.BuildAuthorizationUri(command.ReturnUrl, out var state);

        return Task.FromResult(new FitbitAuthorizationRedirect(uri, state));
    }
}