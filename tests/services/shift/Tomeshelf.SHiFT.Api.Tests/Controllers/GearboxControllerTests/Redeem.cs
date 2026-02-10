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

public class Redeem
{
    /// <summary>
    ///     Returns bad request when the code is missing.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ReturnsBadRequest_WhenCodeMissing()
    {
        // Arrange
        var redeemHandler = A.Fake<ICommandHandler<RedeemShiftCodeCommand, IReadOnlyList<RedeemResult>>>();
        var sweepHandler = A.Fake<ICommandHandler<SweepShiftKeysCommand, ShiftKeySweepResult>>();
        var options = new TestOptionsMonitor<ShiftKeyScannerOptions>(new ShiftKeyScannerOptions());
        var controller = new GearboxController(redeemHandler, sweepHandler, options);

        // Act
        var result = await controller.Redeem(new RedeemRequestDto("   "), CancellationToken.None);

        // Assert
        var badRequest = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequest.Value.ShouldBe("Code is required.");
        A.CallTo(() => redeemHandler.Handle(A<RedeemShiftCodeCommand>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }

    /// <summary>
    ///     Returns the summary and results.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ReturnsSummaryAndResults()
    {
        // Arrange
        var redeemHandler = A.Fake<ICommandHandler<RedeemShiftCodeCommand, IReadOnlyList<RedeemResult>>>();
        var sweepHandler = A.Fake<ICommandHandler<SweepShiftKeysCommand, ShiftKeySweepResult>>();
        var options = new TestOptionsMonitor<ShiftKeyScannerOptions>(new ShiftKeyScannerOptions());
        var controller = new GearboxController(redeemHandler, sweepHandler, options);
        var results = new List<RedeemResult>
        {
            new RedeemResult(1, "user@example.com", "psn", true, null, null),
            new RedeemResult(2, "user2@example.com", "steam", false, RedeemErrorCode.Unknown, "failed")
        };

        A.CallTo(() => redeemHandler.Handle(A<RedeemShiftCodeCommand>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<RedeemResult>>(results));

        // Act
        var result = await controller.Redeem(new RedeemRequestDto("CODE-123"), CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        var payload = ok.Value.ShouldBeOfType<RedeemResponseDto>();
        payload.Summary.Total.ShouldBe(2);
        payload.Summary.Succeeded.ShouldBe(1);
        payload.Summary.Failed.ShouldBe(1);
        payload.Results.ShouldBe(results);
    }
}