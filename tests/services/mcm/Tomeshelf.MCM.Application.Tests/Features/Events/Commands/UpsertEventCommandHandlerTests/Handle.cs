using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.MCM.Application.Features.Events.Commands;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Tests.Features.Events.Commands.UpsertEventCommandHandlerTests;

public class Handle
{
    [Fact]
    public async Task EventServiceThrowsException_ExceptionIsPropagated()
    {
        // Arrange
        var faker = new Faker();
        var service = A.Fake<IEventService>();
        var handler = new UpsertEventCommandHandler(service);
        var model = new EventConfigModel
        {
            Id = faker.Random.AlphaNumeric(8),
            Name = faker.Company.CompanyName()
        };
        var command = new UpsertEventCommand(model);
        var expectedException = new InvalidOperationException(faker.Lorem.Sentence());

        A.CallTo(() => service.UpsertAsync(model, A<CancellationToken>._))
         .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(act);
        exception.Message.ShouldBe(expectedException.Message);
        A.CallTo(() => service.UpsertAsync(model, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ValidCommand_CallsUpsertAsyncAndReturnsTrue()
    {
        // Arrange
        var faker = new Faker();
        var service = A.Fake<IEventService>();
        var handler = new UpsertEventCommandHandler(service);
        var model = new EventConfigModel
        {
            Id = faker.Random.AlphaNumeric(8),
            Name = faker.Company.CompanyName()
        };
        var command = new UpsertEventCommand(model);

        A.CallTo(() => service.UpsertAsync(model, A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        A.CallTo(() => service.UpsertAsync(model, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}