using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Features.Redemption.Commands;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;

namespace Tomeshelf.SHiFT.Application.Tests.Features.Redemption.Commands.RedeemShiftCodeCommandHandlerTests;

public class Handle
{
    [Fact]
    public async Task CallsGearboxClientAndReturnsResults()
    {
        var faker = new Faker();
        var gearbox = A.Fake<IGearboxClient>();
        var handler = new RedeemShiftCodeCommandHandler(gearbox);
        var code = "CODE-12345-ABCDE-67890-FGHIJ";
        IReadOnlyList<RedeemResult> expected = new List<RedeemResult> { new RedeemResult(1, faker.Internet.Email(), "psn", true, null, null) };

        A.CallTo(() => gearbox.RedeemCodeAsync(code, A<CancellationToken>._))
         .Returns(Task.FromResult(expected));

        var result = await handler.Handle(new RedeemShiftCodeCommand(code), CancellationToken.None);

        result.ShouldBeSameAs(expected);
        A.CallTo(() => gearbox.RedeemCodeAsync(code, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}