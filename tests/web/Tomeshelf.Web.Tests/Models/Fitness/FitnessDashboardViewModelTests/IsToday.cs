using System;
using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.FitnessDashboardViewModelTests;

public class IsToday
{
    /// <summary>
    ///     Returns true when the selected date matches today.
    /// </summary>
    [Fact]
    public void WhenSelectedDateMatchesToday_ReturnsTrue()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today)
                            .ToString("yyyy-MM-dd");

        // Act
        var model = FitnessDashboardViewModel.Empty(today, WeightUnit.Stones);

        // Assert
        model.IsToday.ShouldBeTrue();
        model.CanRefresh.ShouldBeFalse();
    }
}