using System.ComponentModel.DataAnnotations;
using Tomeshelf.SHiFT.Application;

namespace Tomeshelf.SHiFT.Application.Tests.ShiftKeyScannerOptionsTests;

public class Validation
{
    [Fact]
    public void Validation_FailsForOutOfRangeLookback()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions { LookbackHours = 0 };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, new ValidationContext(options), results, true);

        // Assert
        isValid.ShouldBeFalse();
        results.ShouldContain(result => result.MemberNames.Contains(nameof(ShiftKeyScannerOptions.LookbackHours)));
    }

    [Fact]
    public void Validation_FailsForOutOfRangeXOptions()
    {
        // Arrange
        var options = new ShiftKeyScannerOptions.XSourceOptions { TokenCacheMinutes = 0 };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, new ValidationContext(options), results, true);

        // Assert
        isValid.ShouldBeFalse();
        results.ShouldContain(result => result.MemberNames.Contains(nameof(ShiftKeyScannerOptions.XSourceOptions.TokenCacheMinutes)));
    }
}
