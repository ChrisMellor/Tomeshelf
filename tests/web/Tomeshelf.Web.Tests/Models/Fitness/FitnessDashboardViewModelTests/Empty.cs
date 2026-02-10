using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.FitnessDashboardViewModelTests;

public class Empty
{
    /// <summary>
    ///     Sets the dates and flags.
    /// </summary>
    [Fact]
    public void SetsDatesAndFlags()
    {
        // Arrange
        var selectedDate = "2020-01-01";

        // Act
        var model = FitnessDashboardViewModel.Empty(selectedDate, WeightUnit.Pounds, "error");

        // Assert
        model.SelectedDate.ShouldBe(selectedDate);
        model.NextDate.ShouldBe("2020-01-02");
        model.PreviousDate.ShouldBe("2019-12-31");
        model.HasData.ShouldBeFalse();
        model.ErrorMessage.ShouldBe("error");
        model.Unit.ShouldBe(WeightUnit.Pounds);
    }
}