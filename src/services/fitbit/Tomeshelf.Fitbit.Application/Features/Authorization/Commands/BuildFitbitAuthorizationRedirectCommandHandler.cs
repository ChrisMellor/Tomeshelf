using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Fitbit.Application.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;

namespace Tomeshelf.Fitbit.Application.Features.Authorization.Commands;

public sealed class BuildFitbitAuthorizationRedirectCommandHandler : ICommandHandler<BuildFitbitAuthorizationRedirectCommand, FitbitAuthorizationRedirect>
{
    private readonly IFitbitAuthorizationService _authorizationService;

    public BuildFitbitAuthorizationRedirectCommandHandler(IFitbitAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public Task<FitbitAuthorizationRedirect> Handle(BuildFitbitAuthorizationRedirectCommand command, CancellationToken cancellationToken)
    {
        var uri = _authorizationService.BuildAuthorizationUri(command.ReturnUrl, out var state);
        return Task.FromResult(new FitbitAuthorizationRedirect(uri, state));
    }
}
