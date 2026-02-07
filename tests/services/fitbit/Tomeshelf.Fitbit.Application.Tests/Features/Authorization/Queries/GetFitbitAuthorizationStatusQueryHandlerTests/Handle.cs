using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Queries;

namespace Tomeshelf.Fitbit.Application.Tests.Features.Authorization.Queries.GetFitbitAuthorizationStatusQueryHandlerTests;

public class Handle
{
    [Fact]
    public async Task HasAccessTokenAndRefreshToken_ReturnsAuthorizedStatus()
    {
        var faker = new Faker();
        var tokenCache = A.Fake<IFitbitTokenCache>();
        var handler = new GetFitbitAuthorizationStatusQueryHandler(tokenCache);
        var accessToken = faker.Random.AlphaNumeric(16);
        var refreshToken = faker.Random.AlphaNumeric(16);

        A.CallTo(() => tokenCache.AccessToken)
         .Returns(accessToken);
        A.CallTo(() => tokenCache.RefreshToken)
         .Returns(refreshToken);

        var result = await handler.Handle(new GetFitbitAuthorizationStatusQuery(), CancellationToken.None);

        result.HasAccessToken.ShouldBeTrue();
        result.HasRefreshToken.ShouldBeTrue();
    }

    [Fact]
    public async Task NoAccessToken_ReturnsUnauthorizedStatus()
    {
        var faker = new Faker();
        var tokenCache = A.Fake<IFitbitTokenCache>();
        var handler = new GetFitbitAuthorizationStatusQueryHandler(tokenCache);
        var refreshToken = faker.Random.AlphaNumeric(16);

        A.CallTo(() => tokenCache.AccessToken)
         .Returns(string.Empty);
        A.CallTo(() => tokenCache.RefreshToken)
         .Returns(refreshToken);

        var result = await handler.Handle(new GetFitbitAuthorizationStatusQuery(), CancellationToken.None);

        result.HasAccessToken.ShouldBeFalse();
        result.HasRefreshToken.ShouldBeTrue();
    }

    [Fact]
    public async Task NoRefreshToken_ReturnsNoRefreshStatus()
    {
        var faker = new Faker();
        var tokenCache = A.Fake<IFitbitTokenCache>();
        var handler = new GetFitbitAuthorizationStatusQueryHandler(tokenCache);
        var accessToken = faker.Random.AlphaNumeric(16);

        A.CallTo(() => tokenCache.AccessToken)
         .Returns(accessToken);
        A.CallTo(() => tokenCache.RefreshToken)
         .Returns(string.Empty);

        var result = await handler.Handle(new GetFitbitAuthorizationStatusQuery(), CancellationToken.None);

        result.HasAccessToken.ShouldBeTrue();
        result.HasRefreshToken.ShouldBeFalse();
    }

    [Fact]
    public async Task NoTokens_ReturnsFullyUnauthorizedStatus()
    {
        var tokenCache = A.Fake<IFitbitTokenCache>();
        var handler = new GetFitbitAuthorizationStatusQueryHandler(tokenCache);

        A.CallTo(() => tokenCache.AccessToken)
         .Returns(string.Empty);
        A.CallTo(() => tokenCache.RefreshToken)
         .Returns(string.Empty);

        var result = await handler.Handle(new GetFitbitAuthorizationStatusQuery(), CancellationToken.None);

        result.HasAccessToken.ShouldBeFalse();
        result.HasRefreshToken.ShouldBeFalse();
    }
}