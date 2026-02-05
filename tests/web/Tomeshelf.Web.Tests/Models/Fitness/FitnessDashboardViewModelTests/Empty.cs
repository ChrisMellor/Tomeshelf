using FluentAssertions;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.FitnessDashboardViewModelTests;

public class Empty
{
    [Fact]
    public void SetsDatesAndFlags()
    {
        // Arrange
        var selectedDate = "2020-01-01";

        // Act
        var model = FitnessDashboardViewModel.Empty(selectedDate, WeightUnit.Pounds, "error");

        // Assert
        model.SelectedDate
             .Should()
             .Be(selectedDate);
        model.NextDate
             .Should()
             .Be("2020-01-02");
        model.PreviousDate
             .Should()
             .Be("2019-12-31");
        model.HasData
             .Should()
             .BeFalse();
        model.ErrorMessage
             .Should()
             .Be("error");
        model.Unit
             .Should()
             .Be(WeightUnit.Pounds);
    }
}