using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Api.Contracts;
using Tomeshelf.SHiFT.Api.Controllers;
using Tomeshelf.SHiFT.Api.Tests.TestUtilities;
using Tomeshelf.SHiFT.Application;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Commands;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;
using Tomeshelf.SHiFT.Application.Features.Redemption.Commands;
using Tomeshelf.SHiFT.Application.Features.Redemption.Models;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;

namespace Tomeshelf.SHiFT.Api.Tests.Controllers.GearboxControllerTests;

public class Sweep
{
    [Fact]
    public async Task ClampsHours_ToMinimum()
    {
        // Arrange
        var redeemHandler = A.Fake<ICommandHandler<RedeemShiftCodeCommand, IReadOnlyList<RedeemResult>>>();
        var sweepHandler = A.Fake<ICommandHandler<SweepShiftKeysCommand, ShiftKeySweepResult>>();
        var options = new TestOptionsMonitor<ShiftKeyScannerOptions>(new ShiftKeyScannerOptions { LookbackHours = 24 });
        var controller = new GearboxController(redeemHandler, sweepHandler, options);

        var resultPayload = new ShiftKeySweepResult(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, new ShiftKeySweepSummary(0, 0, 0, 0), new List<ShiftKeySweepItem>());

        SweepShiftKeysCommand? captured = null;
        A.CallTo(() => sweepHandler.Handle(A<SweepShiftKeysCommand>._, A<CancellationToken>._))
         .Invokes(call => captured = call.GetArgument<SweepShiftKeysCommand>(0))
         .Returns(Task.FromResult(resultPayload));

        // Act
        var result = await controller.Sweep(0, CancellationToken.None);

        // Assert
        captured.ShouldNotBeNull();
        captured!.Lookback.ShouldBe(TimeSpan.FromHours(1));
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ClampsHoursAndMapsResponse()
    {
        // Arrange
        var redeemHandler = A.Fake<ICommandHandler<RedeemShiftCodeCommand, IReadOnlyList<RedeemResult>>>();
        var sweepHandler = A.Fake<ICommandHandler<SweepShiftKeysCommand, ShiftKeySweepResult>>();
        var options = new TestOptionsMonitor<ShiftKeyScannerOptions>(new ShiftKeyScannerOptions { LookbackHours = 6 });
        var controller = new GearboxController(redeemHandler, sweepHandler, options);

        var since = new DateTimeOffset(2025, 01, 01, 00, 00, 00, TimeSpan.Zero);
        var scanned = since.AddMinutes(30);
        var itemResults = new List<RedeemResult>
        {
            new RedeemResult(1, "user@example.com", "psn", true, null, null),
            new RedeemResult(2, "user2@example.com", "psn", false, RedeemErrorCode.Unknown, "failed")
        };
        var resultPayload = new ShiftKeySweepResult(since, scanned, new ShiftKeySweepSummary(1, 2, 1, 1), new List<ShiftKeySweepItem> { new ShiftKeySweepItem("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY", new[] { "x" }, itemResults) });

        SweepShiftKeysCommand? captured = null;
        A.CallTo(() => sweepHandler.Handle(A<SweepShiftKeysCommand>._, A<CancellationToken>._))
         .Invokes(call => captured = call.GetArgument<SweepShiftKeysCommand>(0))
         .Returns(Task.FromResult(resultPayload));

        // Act
        var result = await controller.Sweep(200, CancellationToken.None);

        // Assert
        captured.ShouldNotBeNull();
        captured!.Lookback.ShouldBe(TimeSpan.FromHours(168));

        var ok = result.ShouldBeOfType<OkObjectResult>();
        var payload = ok.Value.ShouldBeOfType<ShiftKeySweepResponseDto>();
        payload.SinceUtc.ShouldBe(since);
        payload.ScannedAtUtc.ShouldBe(scanned);
        payload.Summary.TotalKeys.ShouldBe(1);
        payload.Summary.TotalRedemptionAttempts.ShouldBe(2);
        payload.Items.ShouldHaveSingleItem();
        payload.Items[0]
               .Code
               .ShouldBe("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY");
        payload.Items[0]
               .Summary
               .Total
               .ShouldBe(2);
        payload.Items[0]
               .Summary
               .Succeeded
               .ShouldBe(1);
        payload.Items[0]
               .Summary
               .Failed
               .ShouldBe(1);
    }

    [Fact]
    public async Task UsesDefaultLookback_WhenHoursNull()
    {
        // Arrange
        var redeemHandler = A.Fake<ICommandHandler<RedeemShiftCodeCommand, IReadOnlyList<RedeemResult>>>();
        var sweepHandler = A.Fake<ICommandHandler<SweepShiftKeysCommand, ShiftKeySweepResult>>();
        var options = new TestOptionsMonitor<ShiftKeyScannerOptions>(new ShiftKeyScannerOptions { LookbackHours = 12 });
        var controller = new GearboxController(redeemHandler, sweepHandler, options);

        var resultPayload = new ShiftKeySweepResult(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, new ShiftKeySweepSummary(0, 0, 0, 0), new List<ShiftKeySweepItem>());

        SweepShiftKeysCommand? captured = null;
        A.CallTo(() => sweepHandler.Handle(A<SweepShiftKeysCommand>._, A<CancellationToken>._))
         .Invokes(call => captured = call.GetArgument<SweepShiftKeysCommand>(0))
         .Returns(Task.FromResult(resultPayload));

        // Act
        var result = await controller.Sweep(null, CancellationToken.None);

        // Assert
        captured.ShouldNotBeNull();
        captured!.Lookback.ShouldBe(TimeSpan.FromHours(12));
        result.ShouldBeOfType<OkObjectResult>();
    }
}