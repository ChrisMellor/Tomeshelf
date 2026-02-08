using Bogus;
using Shouldly;
using Tomeshelf.Executor.Models;

namespace Tomeshelf.Executor.Tests.Models.CronBuilderViewModelTests;

public class Constructor
{
    [Fact]
    public void WhenInitialValueNull_UsesEmptyString()
    {
        // Arrange
        var faker = new Faker();
        // Act
        var inputId = faker.Random.Word();
        var inputName = faker.Random.Word();

        // Assert
        var model = new CronBuilderViewModel(inputId, inputName, null);

        model.InitialValue.ShouldBe(string.Empty);
        model.InputId.ShouldBe(inputId);
        model.InputName.ShouldBe(inputName);
    }

    [Fact]
    public void WhenNullInputId_Throws()
    {
        // Arrange
        var faker = new Faker();

        // Act
        Action act = () => new CronBuilderViewModel(null!, faker.Random.Word(), "* * * * *");

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    [Fact]
    public void WhenNullInputName_Throws()
    {
        // Arrange
        var faker = new Faker();

        // Act
        Action act = () => new CronBuilderViewModel(faker.Random.Word(), null!, "* * * * *");

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }
}