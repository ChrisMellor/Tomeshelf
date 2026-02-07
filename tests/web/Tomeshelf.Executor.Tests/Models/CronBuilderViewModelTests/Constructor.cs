using Bogus;
using Shouldly;
using Tomeshelf.Executor.Models;

namespace Tomeshelf.Executor.Tests.Models.CronBuilderViewModelTests;

public class Constructor
{
    [Fact]
    public void WhenInitialValueNull_UsesEmptyString()
    {
        var faker = new Faker();
        var inputId = faker.Random.Word();
        var inputName = faker.Random.Word();

        var model = new CronBuilderViewModel(inputId, inputName, null);

        model.InitialValue.ShouldBe(string.Empty);
        model.InputId.ShouldBe(inputId);
        model.InputName.ShouldBe(inputName);
    }

    [Fact]
    public void WhenNullInputId_Throws()
    {
        var faker = new Faker();

        Action act = () => new CronBuilderViewModel(null!, faker.Random.Word(), "* * * * *");

        Should.Throw<ArgumentNullException>(act);
    }

    [Fact]
    public void WhenNullInputName_Throws()
    {
        var faker = new Faker();

        Action act = () => new CronBuilderViewModel(faker.Random.Word(), null!, "* * * * *");

        Should.Throw<ArgumentNullException>(act);
    }
}