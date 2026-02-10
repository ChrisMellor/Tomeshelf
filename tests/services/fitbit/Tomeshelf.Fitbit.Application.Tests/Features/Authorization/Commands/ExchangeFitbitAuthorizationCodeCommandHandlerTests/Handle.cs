using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Commands;

namespace Tomeshelf.Fitbit.Application.Tests.Features.Authorization.Commands.ExchangeFitbitAuthorizationCodeCommandHandlerTests;

public class Handle
{
    /// <summary>
    ///     The exception is propagated when exchanging the authorization code throws.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExchangeAuthorizationCodeThrows_ExceptionIsPropagated()
    {
        // Arrange
        var faker = new Faker();
        var authorizationService = A.Fake<IFitbitAuthorizationService>();
        var handler = new ExchangeFitbitAuthorizationCodeCommandHandler(authorizationService);
        var codeVerifier = faker.Random.AlphaNumeric(24);
        var returnUrl = "/dashboard";
        var command = new ExchangeFitbitAuthorizationCodeCommand(faker.Random.AlphaNumeric(10), faker.Random.AlphaNumeric(10));
        var expectedException = new InvalidOperationException(faker.Lorem.Sentence());
        string outCodeVerifier = null!;
        string outReturnUrl = null!;

        A.CallTo(() => authorizationService.TryConsumeState(command.State!, out outCodeVerifier, out outReturnUrl))
         .Returns(true)
         .AssignsOutAndRefParameters(codeVerifier, returnUrl);

        A.CallTo(() => authorizationService.ExchangeAuthorizationCodeAsync(command.Code, codeVerifier, A<CancellationToken>._))
         .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(act);
        exception.Message.ShouldBe(expectedException.Message);
        A.CallTo(() => authorizationService.TryConsumeState(command.State!, out outCodeVerifier, out outReturnUrl))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => authorizationService.ExchangeAuthorizationCodeAsync(command.Code, codeVerifier, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    /// <summary>
    ///     Returns error result when the try consume state returns false.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task TryConsumeStateReturnsFalse_ReturnsErrorResult()
    {
        // Arrange
        var faker = new Faker();
        var authorizationService = A.Fake<IFitbitAuthorizationService>();
        var handler = new ExchangeFitbitAuthorizationCodeCommandHandler(authorizationService);
        var command = new ExchangeFitbitAuthorizationCodeCommand(faker.Random.AlphaNumeric(10), faker.Random.AlphaNumeric(10));
        string outCodeVerifier = null!;
        string outReturnUrl = null!;

        A.CallTo(() => authorizationService.TryConsumeState(command.State!, out outCodeVerifier, out outReturnUrl))
         .Returns(false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsInvalidState.ShouldBeTrue();
        result.ReturnUrl.ShouldBe("/fitness");
        A.CallTo(() => authorizationService.TryConsumeState(command.State!, out outCodeVerifier, out outReturnUrl))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => authorizationService.ExchangeAuthorizationCodeAsync(A<string>._, A<string>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }

    /// <summary>
    ///     Calls exchange and returns success result when the try consume state returns true.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task TryConsumeStateReturnsTrue_CallsExchangeAndReturnsSuccessResult()
    {
        // Arrange
        var faker = new Faker();
        var authorizationService = A.Fake<IFitbitAuthorizationService>();
        var handler = new ExchangeFitbitAuthorizationCodeCommandHandler(authorizationService);
        var codeVerifier = faker.Random.AlphaNumeric(24);
        var returnUrl = "/dashboard";
        var command = new ExchangeFitbitAuthorizationCodeCommand(faker.Random.AlphaNumeric(10), faker.Random.AlphaNumeric(10));
        string outCodeVerifier = null!;
        string outReturnUrl = null!;

        A.CallTo(() => authorizationService.TryConsumeState(command.State!, out outCodeVerifier, out outReturnUrl))
         .Returns(true)
         .AssignsOutAndRefParameters(codeVerifier, returnUrl);

        A.CallTo(() => authorizationService.ExchangeAuthorizationCodeAsync(command.Code, codeVerifier, A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsInvalidState.ShouldBeFalse();
        result.ReturnUrl.ShouldBe(returnUrl);
        A.CallTo(() => authorizationService.TryConsumeState(command.State!, out outCodeVerifier, out outReturnUrl))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => authorizationService.ExchangeAuthorizationCodeAsync(command.Code, codeVerifier, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}