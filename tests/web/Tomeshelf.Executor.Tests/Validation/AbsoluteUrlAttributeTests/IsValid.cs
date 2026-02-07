using System.ComponentModel.DataAnnotations;
using Shouldly;
using Tomeshelf.Executor.Validation;

namespace Tomeshelf.Executor.Tests.Validation.AbsoluteUrlAttributeTests;

public class IsValid
{
    private readonly AbsoluteUrlAttribute _attribute = new();

    [Theory]
    [InlineData("http:///")]
    [InlineData("::::")]
    public void InvalidInputs_ReturnsError(string value)
    {
        var result = _attribute.GetValidationResult(value, new ValidationContext(new object()) { DisplayName = "Url" });

        result.ShouldNotBe(ValidationResult.Success);
        result.ShouldNotBeNull();
        result!.ErrorMessage.ShouldContain("must be a fully-qualified http, https, or ftp URL");
    }

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
        var result = _attribute.GetValidationResult(value, new ValidationContext(new object()));

        result.ShouldBe(ValidationResult.Success);
    }
}