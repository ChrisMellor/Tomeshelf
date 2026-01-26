using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.SHiFT.Application.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Features.Redemption.Commands;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;
using Tomeshelf.SHiFT.Application.Features.Settings.Queries;

namespace Tomeshelf.SHiFT.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?>, GetShiftSettingsQueryHandler>();
        services.AddScoped<ICommandHandler<CreateShiftSettingsCommand, int>, CreateShiftSettingsCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateShiftSettingsCommand, bool>, UpdateShiftSettingsCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteShiftSettingsCommand, bool>, DeleteShiftSettingsCommandHandler>();
        services.AddScoped<ICommandHandler<RedeemShiftCodeCommand, IReadOnlyList<RedeemResult>>, RedeemShiftCodeCommandHandler>();

        return services;
    }
}
