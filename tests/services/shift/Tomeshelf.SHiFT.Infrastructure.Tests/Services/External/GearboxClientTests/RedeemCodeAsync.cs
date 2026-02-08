using FakeItEasy;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Features.Redemption.Models;
using Tomeshelf.SHiFT.Infrastructure.Services.External;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Services.External.GearboxClientTests;

public class RedeemCodeAsync
{
    public static IEnumerable<object[]> KnownFailures => new List<object[]>
    {
        new object[] { new InvalidOperationException("CSRF token not found in response."), RedeemErrorCode.CsrfMissing, "CSRF token not found." },
        new object[] { new InvalidOperationException("No redemption form found for service."), RedeemErrorCode.NoRedemptionOptions, "No redemption options for that service." },
        new object[] { new HttpRequestException("boom"), RedeemErrorCode.NetworkError, "Network error talking to SHiFT." },
        new object[] { new TaskCanceledException("timeout"), RedeemErrorCode.NetworkError, "Network error talking to SHiFT." },
        new object[] { new InvalidOperationException("unexpected"), RedeemErrorCode.Unknown, "Unexpected error during redemption." }
    };

    [Theory]
    [MemberData(nameof(KnownFailures))]
    public async Task MapsKnownErrors(Exception exception, RedeemErrorCode expectedCode, string expectedMessage)
    {
        // Arrange
        var repository = A.Fake<IShiftSettingsRepository>();
        var sessionFactory = A.Fake<IShiftWebSessionFactory>();
        var session = A.Fake<IShiftWebSession>();
        var client = new GearboxClient(repository, sessionFactory);

        var users = new List<(int Id, string Email, string Password, string Service)> { (8, "user@example.com", "password", "psn") };

        A.CallTo(() => repository.GetForUseAsync(A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<(int Id, string Email, string Password, string Service)>>(users));
        A.CallTo(() => sessionFactory.Create())
         .Returns(session);
        A.CallTo(() => session.GetCsrfFromHomeAsync(A<CancellationToken>._))
         .ThrowsAsync(exception);

        // Act
        var results = await client.RedeemCodeAsync("CODE-123", CancellationToken.None);

        // Assert
        results.ShouldHaveSingleItem();
        results[0]
           .Success
           .ShouldBeFalse();
        results[0]
           .ErrorCode
           .ShouldBe(expectedCode);
        results[0]
           .Message
           .ShouldBe(expectedMessage);
    }

    [Fact]
    public async Task RedeemsAllOptions_AndReturnsSuccess()
    {
        // Arrange
        var repository = A.Fake<IShiftSettingsRepository>();
        var sessionFactory = A.Fake<IShiftWebSessionFactory>();
        var session = A.Fake<IShiftWebSession>();
        var client = new GearboxClient(repository, sessionFactory);

        var users = new List<(int Id, string Email, string Password, string Service)> { (7, "user@example.com", "password", "psn") };

        A.CallTo(() => repository.GetForUseAsync(A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<(int Id, string Email, string Password, string Service)>>(users));
        A.CallTo(() => sessionFactory.Create())
         .Returns(session);
        A.CallTo(() => session.GetCsrfFromHomeAsync(A<CancellationToken>._))
         .Returns(Task.FromResult("csrf-home"));
        A.CallTo(() => session.LoginAsync("user@example.com", "password", "csrf-home", A<CancellationToken>._))
         .Returns(Task.CompletedTask);
        A.CallTo(() => session.GetCsrfFromRewardsAsync("csrf-home", "user@example.com", "password", A<CancellationToken>._))
         .Returns(Task.FromResult("csrf-rewards"));

        var options = new List<RedemptionOption>
        {
            new("psn", "Title", null, "body-1"),
            new("psn", "Title", null, "body-2")
        };

        A.CallTo(() => session.BuildRedeemBodyAsync("CODE-123", "csrf-rewards", "psn", A<CancellationToken>._))
         .Returns(Task.FromResult(options));
        A.CallTo(() => session.RedeemAsync(A<string>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        // Act
        var results = await client.RedeemCodeAsync("  CODE-123 ", CancellationToken.None);

        // Assert
        results.ShouldHaveSingleItem();
        results[0]
           .Success
           .ShouldBeTrue();
        A.CallTo(() => session.BuildRedeemBodyAsync("CODE-123", "csrf-rewards", "psn", A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => session.RedeemAsync("body-1", A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => session.RedeemAsync("body-2", A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ReturnsAccountMisconfigured_WhenUserMissingFields()
    {
        // Arrange
        var repository = A.Fake<IShiftSettingsRepository>();
        var sessionFactory = A.Fake<IShiftWebSessionFactory>();
        var client = new GearboxClient(repository, sessionFactory);

        var users = new List<(int Id, string Email, string Password, string Service)> { (1, string.Empty, "password", "psn") };

        A.CallTo(() => repository.GetForUseAsync(A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<(int Id, string Email, string Password, string Service)>>(users));

        // Act
        var results = await client.RedeemCodeAsync("CODE-12345", CancellationToken.None);

        // Assert
        results.ShouldHaveSingleItem();
        results[0]
           .Success
           .ShouldBeFalse();
        results[0]
           .ErrorCode
           .ShouldBe(RedeemErrorCode.AccountMisconfigured);
        results[0]
           .Message
           .ShouldBe("Missing email, password, or service.");
        A.CallTo(() => sessionFactory.Create())
         .MustNotHaveHappened();
    }

    [Fact]
    public async Task ReturnsEmpty_WhenNoUsersConfigured()
    {
        // Arrange
        var repository = A.Fake<IShiftSettingsRepository>();
        var sessionFactory = A.Fake<IShiftWebSessionFactory>();
        var client = new GearboxClient(repository, sessionFactory);

        A.CallTo(() => repository.GetForUseAsync(A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<(int Id, string Email, string Password, string Service)>>(new List<(int, string, string, string)>()));

        // Act
        var results = await client.RedeemCodeAsync("CODE-123", CancellationToken.None);

        // Assert
        results.ShouldBeEmpty();
        A.CallTo(() => sessionFactory.Create())
         .MustNotHaveHappened();
    }

    [Fact]
    public async Task Throws_WhenCodeMissing()
    {
        // Arrange
        var repository = A.Fake<IShiftSettingsRepository>();
        var sessionFactory = A.Fake<IShiftWebSessionFactory>();
        var client = new GearboxClient(repository, sessionFactory);

        // Act
        var action = () => client.RedeemCodeAsync("   ", CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ArgumentException>(action);
    }
}