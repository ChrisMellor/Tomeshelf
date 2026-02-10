using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Queries;

namespace Tomeshelf.Fitbit.Application.Tests.Features.Authorization.Queries.GetFitbitAuthorizationStatusQueryHandlerTests;

public class Handle
{
    /// <summary>
    ///     Returns authorized status when the has access token and refresh token.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task HasAccessTokenAndRefreshToken_ReturnsAuthorizedStatus()
    {
        // Arrange
        var faker = new Faker();
        var tokenCache = A.Fake<IFitbitTokenCache>();
        var handler = new GetFitbitAuthorizationStatusQueryHandler(tokenCache);
        var accessToken = faker.Random.AlphaNumeric(16);
        var refreshToken = faker.Random.AlphaNumeric(16);

        A.CallTo(() => tokenCache.AccessToken)
         .Returns(accessToken);
        A.CallTo(() => tokenCache.RefreshToken)
         .Returns(refreshToken);

        // Act
        var result = await handler.Handle(new GetFitbitAuthorizationStatusQuery(), CancellationToken.None);

        // Assert
        result.HasAccessToken.ShouldBeTrue();
        result.HasRefreshToken.ShouldBeTrue();
    }

    /// <summary>
    ///     Returns unauthorized status when there is no access token.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task NoAccessToken_ReturnsUnauthorizedStatus()
    {
        // Arrange
        var faker = new Faker();
        var tokenCache = A.Fake<IFitbitTokenCache>();
        var handler = new GetFitbitAuthorizationStatusQueryHandler(tokenCache);
        var refreshToken = faker.Random.AlphaNumeric(16);

        A.CallTo(() => tokenCache.AccessToken)
         .Returns(string.Empty);
        A.CallTo(() => tokenCache.RefreshToken)
         .Returns(refreshToken);

        // Act
        var result = await handler.Handle(new GetFitbitAuthorizationStatusQuery(), CancellationToken.None);

        // Assert
        result.HasAccessToken.ShouldBeFalse();
        result.HasRefreshToken.ShouldBeTrue();
    }

    /// <summary>
    ///     Returns no refresh status when there is no refresh token.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task NoRefreshToken_ReturnsNoRefreshStatus()
    {
        // Arrange
        var faker = new Faker();
        var tokenCache = A.Fake<IFitbitTokenCache>();
        var handler = new GetFitbitAuthorizationStatusQueryHandler(tokenCache);
        var accessToken = faker.Random.AlphaNumeric(16);

        A.CallTo(() => tokenCache.AccessToken)
         .Returns(accessToken);
        A.CallTo(() => tokenCache.RefreshToken)
         .Returns(string.Empty);

        // Act
        var result = await handler.Handle(new GetFitbitAuthorizationStatusQuery(), CancellationToken.None);

        // Assert
        result.HasAccessToken.ShouldBeTrue();
        result.HasRefreshToken.ShouldBeFalse();
    }

    /// <summary>
    ///     Returns fully unauthorized status when there are no tokens.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task NoTokens_ReturnsFullyUnauthorizedStatus()
    {
        // Arrange
        var tokenCache = A.Fake<IFitbitTokenCache>();
        var handler = new GetFitbitAuthorizationStatusQueryHandler(tokenCache);

        A.CallTo(() => tokenCache.AccessToken)
         .Returns(string.Empty);
        A.CallTo(() => tokenCache.RefreshToken)
         .Returns(string.Empty);

        // Act
        var result = await handler.Handle(new GetFitbitAuthorizationStatusQuery(), CancellationToken.None);

        // Assert
        result.HasAccessToken.ShouldBeFalse();
        result.HasRefreshToken.ShouldBeFalse();
    }
}