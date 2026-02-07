using System.ComponentModel.DataAnnotations;
using Shouldly;

namespace Tomeshelf.SHiFT.Application.Tests.ShiftKeyScannerOptionsTests;

public class Validation
{
    [Fact]
    public void Validation_FailsForOutOfRangeLookback()
    {
        var options = new ShiftKeyScannerOptions { LookbackHours = 0 };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(options, new ValidationContext(options), results, true);

        isValid.ShouldBeFalse();
        results.ShouldContain(result => result.MemberNames.Contains(nameof(ShiftKeyScannerOptions.LookbackHours)));
    }

    [Fact]
    public void Validation_FailsForOutOfRangeXOptions()
    {
        var options = new ShiftKeyScannerOptions.XSourceOptions { TokenCacheMinutes = 0 };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(options, new ValidationContext(options), results, true);

        isValid.ShouldBeFalse();
        results.ShouldContain(result => result.MemberNames.Contains(nameof(ShiftKeyScannerOptions.XSourceOptions.TokenCacheMinutes)));
    }
}