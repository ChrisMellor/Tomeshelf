using Bogus;
using FakeItEasy;
using FluentAssertions;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Commands;

namespace Tomeshelf.Fitbit.Application.Tests.Features.Authorization.Commands.ExchangeFitbitAuthorizationCodeCommandHandlerTests;

public class Handle
{
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
        var exception = await act.Should()
                                 .ThrowAsync<InvalidOperationException>();
        exception.WithMessage(expectedException.Message);
        A.CallTo(() => authorizationService.TryConsumeState(command.State!, out outCodeVerifier, out outReturnUrl))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => authorizationService.ExchangeAuthorizationCodeAsync(command.Code, codeVerifier, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

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
        result.IsInvalidState
              .Should()
              .BeTrue();
        result.ReturnUrl
              .Should()
              .Be("/fitness");
        A.CallTo(() => authorizationService.TryConsumeState(command.State!, out outCodeVerifier, out outReturnUrl))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => authorizationService.ExchangeAuthorizationCodeAsync(A<string>._, A<string>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }

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
        result.IsInvalidState
              .Should()
              .BeFalse();
        result.ReturnUrl
              .Should()
              .Be(returnUrl);
        A.CallTo(() => authorizationService.TryConsumeState(command.State!, out outCodeVerifier, out outReturnUrl))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => authorizationService.ExchangeAuthorizationCodeAsync(command.Code, codeVerifier, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}