using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Tomeshelf.Executor.Validation;

namespace Tomeshelf.Executor.Tests.Validation.AbsoluteUrlAttributeTests;

public class IsValid
{
    private readonly AbsoluteUrlAttribute _attribute = new();

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path")]
    [InlineData("ftp://files.example.com")]
    [InlineData("example.com")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidInputs_ReturnsSuccess(string? value)
    {
        // Act
        var result = _attribute.GetValidationResult(value, new ValidationContext(new object()));

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Theory]
    [InlineData("http:///")]
    [InlineData("::::")]
    public void InvalidInputs_ReturnsError(string value)
    {
        // Act
        var result = _attribute.GetValidationResult(value, new ValidationContext(new object()) { DisplayName = "Url" });

        // Assert
        result.Should().NotBe(ValidationResult.Success);
        result!.ErrorMessage.Should().Contain("must be a fully-qualified http, https, or ftp URL");
    }
}
