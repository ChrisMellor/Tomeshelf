using System.ComponentModel.DataAnnotations;
using Shouldly;
using Tomeshelf.Executor.Models;

namespace Tomeshelf.Executor.Tests.Models.EndpointEditorModelTests;

public class Validation
{
    /// <summary>
    ///     Fails the when required fields missing.
    /// </summary>
    [Fact]
    public void FailsWhenRequiredFieldsMissing()
    {
        // Arrange
        var model = new EndpointEditorModel();
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        // Assert
        isValid.ShouldBeFalse();
        results.ShouldNotBeEmpty();
    }
}