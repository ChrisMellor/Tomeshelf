using Bogus;
using FakeItEasy;
using FluentAssertions;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Commands;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;
using Tomeshelf.SHiFT.Application.Features.Redemption.Models;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;

namespace Tomeshelf.SHiFT.Application.Tests.Features.KeyDiscovery.Commands.SweepShiftKeysCommandHandlerTests;

public class Handle
{
    [Fact]
    public async Task GroupsCodesAndBuildsSummary()
    {
        // Arrange
        var faker = new Faker();
        var clock = A.Fake<IClock>();
        var gearbox = A.Fake<IGearboxClient>();
        var sourceA = A.Fake<IShiftKeySource>();
        var sourceB = A.Fake<IShiftKeySource>();

        var now = DateTimeOffset.UtcNow;
        A.CallTo(() => clock.UtcNow)
         .Returns(now);

        A.CallTo(() => sourceA.GetKeysAsync(A<DateTimeOffset>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<ShiftKeyCandidate>>(new List<ShiftKeyCandidate> { new ShiftKeyCandidate("abcde-fghij-klmno-pqrst-uvwxy", "Reddit", now) }));

        A.CallTo(() => sourceB.GetKeysAsync(A<DateTimeOffset>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<ShiftKeyCandidate>>(new List<ShiftKeyCandidate>
          {
              new ShiftKeyCandidate("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY", "twitter", now),
              new ShiftKeyCandidate("11111-22222-33333-44444-55555", "Blog", now)
          }));

        A.CallTo(() => gearbox.RedeemCodeAsync("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY", A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<RedeemResult>>(new List<RedeemResult>
          {
              new RedeemResult(1, faker.Internet.Email(), "psn", true, null, null),
              new RedeemResult(2, faker.Internet.Email(), "psn", false, RedeemErrorCode.RedemptionFailed, "used")
          }));

        A.CallTo(() => gearbox.RedeemCodeAsync("11111-22222-33333-44444-55555", A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<RedeemResult>>(new List<RedeemResult> { new RedeemResult(3, faker.Internet.Email(), "steam", true, null, null) }));

        var handler = new SweepShiftKeysCommandHandler(new[] { sourceA, sourceB }, gearbox, clock);

        // Act
        var result = await handler.Handle(new SweepShiftKeysCommand(TimeSpan.FromMinutes(5)), CancellationToken.None);

        // Assert
        result.SinceUtc
              .Should()
              .Be(now - TimeSpan.FromHours(1));
        result.Items
              .Should()
              .HaveCount(2);

        var first = result.Items.First(item => item.Code == "ABCDE-FGHIJ-KLMNO-PQRST-UVWXY");
        first.Sources
             .Should()
             .Equal("Reddit", "twitter");
        first.Results
             .Should()
             .HaveCount(2);

        result.Summary
              .TotalKeys
              .Should()
              .Be(2);
        result.Summary
              .TotalRedemptionAttempts
              .Should()
              .Be(3);
        result.Summary
              .TotalSucceeded
              .Should()
              .Be(2);
        result.Summary
              .TotalFailed
              .Should()
              .Be(1);
    }
}