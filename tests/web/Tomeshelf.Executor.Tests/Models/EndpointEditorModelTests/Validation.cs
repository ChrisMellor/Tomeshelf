using System.ComponentModel.DataAnnotations;
using Shouldly;
using Tomeshelf.Executor.Models;

namespace Tomeshelf.Executor.Tests.Models.EndpointEditorModelTests;

public class Validation
{
    [Fact]
    public void FailsWhenRequiredFieldsMissing()
    {
        var model = new EndpointEditorModel();
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        isValid.ShouldBeFalse();
        results.ShouldNotBeEmpty();
    }
}