using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Tomeshelf.Executor.Models;
using Xunit;

namespace Tomeshelf.Executor.Tests.Models;

public class EndpointEditorModelTests
{
    [Fact]
    public void Defaults_AreExpected()
    {
        var model = new EndpointEditorModel();

        Assert.Equal("POST", model.Method);
        Assert.True(model.Enabled);
    }

    [Fact]
    public void Validation_FailsWhenRequiredFieldsMissing()
    {
        var model = new EndpointEditorModel();
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.NotEmpty(results);
    }
}
