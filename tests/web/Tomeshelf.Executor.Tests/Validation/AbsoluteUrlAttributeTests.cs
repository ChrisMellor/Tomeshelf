using System.ComponentModel.DataAnnotations;
using Tomeshelf.Executor.Validation;
using Xunit;

namespace Tomeshelf.Executor.Tests.Validation;

public class AbsoluteUrlAttributeTests
{
    private readonly AbsoluteUrlAttribute _attribute = new();

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path")]
    [InlineData("ftp://files.example.com")]
    [InlineData("example.com")] // Should be treated as http
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void IsValid_ValidInputs_ReturnsSuccess(string? value)
    {
        // Act
        var result = _attribute.GetValidationResult(value, new ValidationContext(new object()));

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("http:///")]
    [InlineData("::::")]
    public void IsValid_InvalidInputs_ReturnsError(string value)
    {
        // Act
        var result = _attribute.GetValidationResult(value, new ValidationContext(new object { }) { DisplayName = "Url" });

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("must be a fully-qualified http, https, or ftp URL", result!.ErrorMessage);
    }
}
