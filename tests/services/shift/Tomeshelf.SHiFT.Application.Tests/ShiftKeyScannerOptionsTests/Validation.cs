using System.ComponentModel.DataAnnotations;
using Shouldly;

namespace Tomeshelf.SHiFT.Application.Tests.ShiftKeyScannerOptionsTests;

public class Validation
{
    /// <summary>
    ///     Fails for out of range lookback when the value is a validation.
    /// </summary>
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

    /// <summary>
    ///     Fails for out of rangex options when the value is a validation.
    /// </summary>
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