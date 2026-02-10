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
    /// <summary>
    ///     Clamps lookback and works.
    /// </summary>
    /// <param name="requestedHours">The requested hours.</param>
    /// <param name="expectedHours">The expected hours.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Theory]
    [InlineData(0.5, 1)]
    [InlineData(200, 168)]
    public async Task ClampLookback_Works(double requestedHours, int expectedHours)
    {
        // Arrange
        var clock = A.Fake<IClock>();
        var gearbox = A.Fake<IGearboxClient>();
        var source = A.Fake<IShiftKeySource>();
        var now = new DateTimeOffset(2025, 02, 01, 12, 00, 00, TimeSpan.Zero);

        A.CallTo(() => clock.UtcNow)
         .Returns(now);
        A.CallTo(() => source.GetKeysAsync(A<DateTimeOffset>._, A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<ShiftKeyCandidate>>(new List<ShiftKeyCandidate>()));

        var handler = new SweepShiftKeysCommandHandler(new[] { source }, gearbox, clock);

        // Act
        var result = await handler.Handle(new SweepShiftKeysCommand(TimeSpan.FromHours(requestedHours)), CancellationToken.None);

        // Assert
        result.SinceUtc.ShouldBe(now - TimeSpan.FromHours(expectedHours));
    }

    /// <summary>
    ///     Deduplicates sources and ands sorts case insensitive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task DeduplicatesSources_AndSortsCaseInsensitive()
    {
        // Arrange
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

        // Act
        var result = await handler.Handle(new SweepShiftKeysCommand(TimeSpan.FromHours(2)), CancellationToken.None);

        // Assert
        result.Items.ShouldHaveSingleItem();
        result.Items[0]
              .Sources
              .ShouldBe(new[] { "Blog", "Reddit" });
    }

    /// <summary>
    ///     Groups the codes and builds summary.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

        // Act
        var result = await handler.Handle(new SweepShiftKeysCommand(TimeSpan.FromMinutes(5)), CancellationToken.None);

        // Assert
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

    /// <summary>
    ///     Ignores empty codes and ands returns empty summary.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task IgnoresEmptyCodes_AndReturnsEmptySummary()
    {
        // Arrange
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

        // Act
        var result = await handler.Handle(new SweepShiftKeysCommand(TimeSpan.FromHours(2)), CancellationToken.None);

        // Assert
        result.Items.ShouldBeEmpty();
        result.Summary.TotalKeys.ShouldBe(0);
        A.CallTo(() => gearbox.RedeemCodeAsync(A<string>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }
}