using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Commands;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;
using Tomeshelf.SHiFT.Application.Features.Redemption.Models;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;

namespace Tomeshelf.SHiFT.Application.Tests.Features.KeyDiscovery.Commands.SweepShiftKeysCommandHandlerTests;

public class Handle
{
    [Theory]
    [InlineData(0.5, 1)]
    [InlineData(200, 168)]
    public async Task ClampLookback_Works(double requestedHours, int expectedHours)
    {
        var clock = A.Fake<IClock>();
        var gearbox = A.Fake<IGearboxClient>();
        var source = A.Fake<IShiftKeySource>();
        var now = new DateTimeOffset(2025, 02, 01, 12, 00, 00, TimeSpan.Zero);

        A.CallTo(() => clock.UtcNow)
         .Returns(now);
        A.CallTo(() => source.GetKeysAsync(A<DateTimeOffset>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<ShiftKeyCandidate>>(new List<ShiftKeyCandidate>()));

        var handler = new SweepShiftKeysCommandHandler(new[] { source }, gearbox, clock);

        var result = await handler.Handle(new SweepShiftKeysCommand(TimeSpan.FromHours(requestedHours)), CancellationToken.None);

        result.SinceUtc.ShouldBe(now - TimeSpan.FromHours(expectedHours));
    }

    [Fact]
    public async Task DeduplicatesSources_AndSortsCaseInsensitive()
    {
        var clock = A.Fake<IClock>();
        var gearbox = A.Fake<IGearboxClient>();
        var source = A.Fake<IShiftKeySource>();
        var now = DateTimeOffset.UtcNow;

        A.CallTo(() => clock.UtcNow)
         .Returns(now);
        A.CallTo(() => source.GetKeysAsync(A<DateTimeOffset>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<ShiftKeyCandidate>>(new List<ShiftKeyCandidate>
          {
              new("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY", "Reddit", now),
              new("abcde-fghij-klmno-pqrst-uvwxy", "reddit", now),
              new("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY", "Blog", now)
          }));
        A.CallTo(() => gearbox.RedeemCodeAsync(A<string>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<RedeemResult>>(new List<RedeemResult>()));

        var handler = new SweepShiftKeysCommandHandler(new[] { source }, gearbox, clock);

        var result = await handler.Handle(new SweepShiftKeysCommand(TimeSpan.FromHours(2)), CancellationToken.None);

        result.Items.ShouldHaveSingleItem();
        result.Items[0]
              .Sources
              .ShouldBe(new[] { "Blog", "Reddit" });
    }

    [Fact]
    public async Task GroupsCodesAndBuildsSummary()
    {
        var faker = new Faker();
        var clock = A.Fake<IClock>();
        var gearbox = A.Fake<IGearboxClient>();
        var sourceA = A.Fake<IShiftKeySource>();
        var sourceB = A.Fake<IShiftKeySource>();

        var now = DateTimeOffset.UtcNow;
        A.CallTo(() => clock.UtcNow)
         .Returns(now);

        A.CallTo(() => sourceA.GetKeysAsync(A<DateTimeOffset>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<ShiftKeyCandidate>>(new List<ShiftKeyCandidate> { new("abcde-fghij-klmno-pqrst-uvwxy", "Reddit", now) }));

        A.CallTo(() => sourceB.GetKeysAsync(A<DateTimeOffset>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<ShiftKeyCandidate>>(new List<ShiftKeyCandidate>
          {
              new("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY", "twitter", now),
              new("11111-22222-33333-44444-55555", "Blog", now)
          }));

        A.CallTo(() => gearbox.RedeemCodeAsync("ABCDE-FGHIJ-KLMNO-PQRST-UVWXY", A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<RedeemResult>>(new List<RedeemResult>
          {
              new(1, faker.Internet.Email(), "psn", true, null, null),
              new(2, faker.Internet.Email(), "psn", false, RedeemErrorCode.RedemptionFailed, "used")
          }));

        A.CallTo(() => gearbox.RedeemCodeAsync("11111-22222-33333-44444-55555", A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<RedeemResult>>(new List<RedeemResult> { new(3, faker.Internet.Email(), "steam", true, null, null) }));

        var handler = new SweepShiftKeysCommandHandler(new[] { sourceA, sourceB }, gearbox, clock);

        var result = await handler.Handle(new SweepShiftKeysCommand(TimeSpan.FromMinutes(5)), CancellationToken.None);

        result.SinceUtc.ShouldBe(now - TimeSpan.FromHours(1));
        result.Items.Count.ShouldBe(2);

        var first = result.Items.First(item => item.Code == "ABCDE-FGHIJ-KLMNO-PQRST-UVWXY");
        first.Sources.ShouldBe(new[] { "Reddit", "twitter" });
        first.Results.Count.ShouldBe(2);

        result.Summary.TotalKeys.ShouldBe(2);
        result.Summary.TotalRedemptionAttempts.ShouldBe(3);
        result.Summary.TotalSucceeded.ShouldBe(2);
        result.Summary.TotalFailed.ShouldBe(1);
    }

    [Fact]
    public async Task IgnoresEmptyCodes_AndReturnsEmptySummary()
    {
        var clock = A.Fake<IClock>();
        var gearbox = A.Fake<IGearboxClient>();
        var source = A.Fake<IShiftKeySource>();
        var now = new DateTimeOffset(2025, 01, 02, 12, 00, 00, TimeSpan.Zero);

        A.CallTo(() => clock.UtcNow)
         .Returns(now);
        A.CallTo(() => source.GetKeysAsync(A<DateTimeOffset>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<ShiftKeyCandidate>>(new List<ShiftKeyCandidate>
          {
              new("   ", "x", now),
              new("", "y", now)
          }));

        var handler = new SweepShiftKeysCommandHandler(new[] { source }, gearbox, clock);

        var result = await handler.Handle(new SweepShiftKeysCommand(TimeSpan.FromHours(2)), CancellationToken.None);

        result.Items.ShouldBeEmpty();
        result.Summary.TotalKeys.ShouldBe(0);
        A.CallTo(() => gearbox.RedeemCodeAsync(A<string>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }
}