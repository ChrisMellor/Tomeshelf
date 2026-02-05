using Bogus;
using FakeItEasy;
using FluentAssertions;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Commands;

namespace Tomeshelf.Fitbit.Application.Tests.Features.Authorization.Commands.BuildFitbitAuthorizationRedirectCommandHandlerTests;

public class Handle
{
    [Fact]
    public async Task BuildAuthorizationUriThrows_ExceptionIsPropagated()
    {
        // Arrange
        var faker = new Faker();
        var authorizationService = A.Fake<IFitbitAuthorizationService>();
        var handler = new BuildFitbitAuthorizationRedirectCommandHandler(authorizationService);
        var returnUrl = faker.Internet.Url();
        var expectedException = new InvalidOperationException(faker.Lorem.Sentence());
        string outState = null!;

        A.CallTo(() => authorizationService.BuildAuthorizationUri(returnUrl, out outState))
         .Throws(expectedException);

        var command = new BuildFitbitAuthorizationRedirectCommand(returnUrl);

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should()
                                 .ThrowAsync<InvalidOperationException>();
        exception.WithMessage(expectedException.Message);
        A.CallTo(() => authorizationService.BuildAuthorizationUri(returnUrl, out outState))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ValidCommand_ReturnsAuthorizationRedirect()
    {
        // Arrange
        var faker = new Faker();
        var authorizationService = A.Fake<IFitbitAuthorizationService>();
        var handler = new BuildFitbitAuthorizationRedirectCommandHandler(authorizationService);
        var returnUrl = faker.Internet.Url();
        var expectedUri = new Uri(faker.Internet.Url());
        var expectedState = faker.Random.AlphaNumeric(16);
        string outState = null!;

        A.CallTo(() => authorizationService.BuildAuthorizationUri(returnUrl, out outState))
         .Returns(expectedUri)
         .AssignsOutAndRefParameters(expectedState);

        var command = new BuildFitbitAuthorizationRedirectCommand(returnUrl);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should()
              .NotBeNull();
        result!.AuthorizationUri
               .Should()
               .Be(expectedUri);
        result.State
              .Should()
              .Be(expectedState);
        A.CallTo(() => authorizationService.BuildAuthorizationUri(returnUrl, out outState))
         .MustHaveHappenedOnceExactly();
    }
}