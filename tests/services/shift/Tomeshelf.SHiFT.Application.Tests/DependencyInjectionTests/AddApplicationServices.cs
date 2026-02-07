using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Commands;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;
using Tomeshelf.SHiFT.Application.Features.Redemption.Commands;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;
using Tomeshelf.SHiFT.Application.Features.Settings.Queries;

namespace Tomeshelf.SHiFT.Application.Tests.DependencyInjectionTests;

public class AddApplicationServices
{
    [Fact]
    public void RegistersHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSingleton(A.Fake<IShiftSettingsRepository>());
        services.AddSingleton(A.Fake<ISecretProtector>());
        services.AddSingleton(A.Fake<IClock>());
        services.AddSingleton(A.Fake<IGearboxClient>());
        services.AddSingleton(A.Fake<IShiftKeySource>());

        // Act
        services.AddApplicationServices();

        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?>>()
                .ShouldBeOfType<GetShiftSettingsQueryHandler>();
        provider.GetRequiredService<ICommandHandler<CreateShiftSettingsCommand, int>>()
                .ShouldBeOfType<CreateShiftSettingsCommandHandler>();
        provider.GetRequiredService<ICommandHandler<UpdateShiftSettingsCommand, bool>>()
                .ShouldBeOfType<UpdateShiftSettingsCommandHandler>();
        provider.GetRequiredService<ICommandHandler<DeleteShiftSettingsCommand, bool>>()
                .ShouldBeOfType<DeleteShiftSettingsCommandHandler>();
        provider.GetRequiredService<ICommandHandler<RedeemShiftCodeCommand, IReadOnlyList<RedeemResult>>>()
                .ShouldBeOfType<RedeemShiftCodeCommandHandler>();
        provider.GetRequiredService<ICommandHandler<SweepShiftKeysCommand, ShiftKeySweepResult>>()
                .ShouldBeOfType<SweepShiftKeysCommandHandler>();
    }
}
