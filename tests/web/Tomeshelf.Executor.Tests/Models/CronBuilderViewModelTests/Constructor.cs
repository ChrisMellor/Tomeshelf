using Bogus;
using Shouldly;
using Tomeshelf.Executor.Models;

namespace Tomeshelf.Executor.Tests.Models.CronBuilderViewModelTests;

public class Constructor
{
    /// <summary>
    ///     Uses empty string when the initial value is null.
    /// </summary>
    [Fact]
    public void WhenInitialValueNull_UsesEmptyString()
    {
        // Arrange
        var faker = new Faker();
        var inputId = faker.Random.Word();
        var inputName = faker.Random.Word();

        // Act
        var model = new CronBuilderViewModel(inputId, inputName, null);

        // Assert
        model.InitialValue.ShouldBe(string.Empty);
        model.InputId.ShouldBe(inputId);
        model.InputName.ShouldBe(inputName);
    }

    /// <summary>
    ///     Throws when the input ID is null.
    /// </summary>
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

    /// <summary>
    ///     Throws when the input name is null.
    /// </summary>
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